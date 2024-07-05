using Microsoft.AspNetCore.Mvc;
using floral_fusion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using floral_fusion.Interfaces;

namespace MvcMovie.Controllers
{
    public class AdminController : Controller
    {
        private readonly IBufferedFileUploadService _fileUploadService;

        public AdminController(IBufferedFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        bool IsAdmin(string email)
        {
            if (email == "") return false;
            XElement xml = XElement.Load("./database/users.xml");
            var user = xml.Elements("User")
                          .FirstOrDefault(u =>
                             u.Element("email")?.Value == email &&
                             u.Element("asAdmin")?.Value == "1");

            return user != null;
        }

        [HttpPost]
        public ActionResult SubmitProduct(Product model, IFormFile imageFile)
        {
            var title = model?.Title;
            var desc = model?.Description;
            var price = model?.Price;
            TempData["variant"] = "danger";
            string filePath = "./database/products.xml";

            if (title == "")
            {
                TempData["message"] = "Title is missing!";
            }
            else if (desc == "")
            {
                TempData["message"] = "Description is missing!";
            }
            else if (price == 0)
            {
                TempData["message"] = "Price is missing!";
            }
            else if (imageFile == null || imageFile.Length == 0)
            {
                TempData["message"] = "Image is missing!";
            }
            else
            {
                // Handle file upload
                var imagePath = _fileUploadService.UploadFile(imageFile);

                // Save product details to XML
                XDocument xmlDoc = XDocument.Load(filePath);
                var rand = new Random();
                XElement product = new("product");
                XElement Id = new("id", rand.Next(100, 100000));
                XElement Title = new("title", title);
                XElement Desc = new("description", desc);
                XElement Image = new("image", imagePath); // Save the uploaded image path
                XElement Price = new("price", price);
                product.Add(Id);
                product.Add(Title);
                product.Add(Desc);
                product.Add(Image);
                product.Add(Price);
                xmlDoc.Element("products")?.Add(product);
                xmlDoc?.Save(filePath);
                TempData["variant"] = "success";
                TempData["message"] = "Successfully Added!";
                return RedirectToAction("NewProduct");
            }

            return RedirectToAction("NewProduct");
        }

        public IActionResult NewProduct()
        {
            TempData["message"] = null;
            return View("NewProduct");
        }

        public IActionResult Users()
        {
            var user = Request?.Cookies["User"];
            if (IsAdmin(user ?? ""))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load("./database/users.xml");
                var usersList = new List<object>();
                var nodes_ = xmlDoc?.SelectNodes("/UsersList/User");
                int itemCount = 0;
                if (nodes_ != null)
                {
                    foreach (XmlNode node in nodes_)
                    {
                        itemCount++;
                        if (node != null)
                        {
                            var product = new
                            {
                                firstname = node?.SelectSingleNode("firstname")?.InnerText,
                                lastname = node?.SelectSingleNode("lastname")?.InnerText,
                                email = node?.SelectSingleNode("email")?.InnerText,
                                createdAt = node?.SelectSingleNode("createdAt")?.InnerText,
                                asAdmin = node?.SelectSingleNode("asAdmin")?.InnerText == "1" ? "Yes" : "No",
                                count = itemCount
                            };
                            usersList.Add(product);
                        }
                    }
                }
                ViewData["User"] = user;
                return View(usersList);
            }
            else
            {
                TempData["variant"] = "danger";
                TempData["Message"] = "Please login as admin!";
                return Redirect("/login?redirect=admin");
            }
        }

        public string DeleteUser(string userEmail)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("./database/users.xml");
            var productNodes = doc?.SelectNodes("/UsersList/User");
            if (productNodes != null)
            {
                foreach (XmlNode productNode in productNodes)
                {
                    var idNode = productNode?.SelectSingleNode("email");
                    if (idNode != null && idNode.InnerText == userEmail)
                    {
                        productNode?.ParentNode?.RemoveChild(productNode);
                        doc?.Save("./database/users.xml");
                        return "success";
                    }
                }
            }
            return "notFound";
        }

        public string DeleteProduct(int itemId)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("./database/products.xml");
            XmlNodeList productNodes = doc?.SelectNodes("/products/product");
            foreach (XmlNode productNode in productNodes)
            {
                XmlNode idNode = productNode?.SelectSingleNode("id");
                if (idNode != null && idNode.InnerText == itemId.ToString())
                {
                    productNode?.ParentNode?.RemoveChild(productNode);
                    doc.Save("./database/products.xml");
                    return "success";
                }
            }
            return "notFound";
        }

        public IActionResult Index()
        {
            var user = Request?.Cookies["User"];
            if (IsAdmin(user ?? ""))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load("./database/products.xml");
                var products = new List<object>();
                var nodes_ = xmlDoc?.SelectNodes("/products/product");
                int itemCount = 0;
                if (nodes_ != null)
                {
                    foreach (XmlNode node in nodes_)
                    {
                        itemCount++;
                        if (node != null)
                        {
                            var product = new
                            {
                                Id = Convert.ToInt32(node?.SelectSingleNode("id")?.InnerText),
                                Title = node?.SelectSingleNode("title")?.InnerText,
                                Image = node?.SelectSingleNode("image")?.InnerText,
                                Price = Convert.ToDecimal(node?.SelectSingleNode("price")?.InnerText),
                                Description = node?.SelectSingleNode("description")?.InnerText,
                                count = itemCount
                            };
                            products.Add(product);
                        }
                    }
                }
                ViewData["User"] = user;
                return View(products);
            }
            else
            {
                TempData["variant"] = "danger";
                TempData["Message"] = "Please login as admin!";
                return Redirect("/login?redirect=admin");
            }

        }
    }
}
