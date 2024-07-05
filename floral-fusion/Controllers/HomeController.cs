using Microsoft.AspNetCore.Mvc;
using floral_fusion.Models;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

namespace floral_fusion.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var user = Request?.Cookies["User"];
            int CartCount = 0;

            // Calculate the number of items in the cart for the logged-in user
            if (user != null)
            {
                var cartItem = new XmlDocument();
                cartItem.Load("./database/cartitem.xml");
                var nodes = cartItem?.SelectNodes("CartData/Item");
                if (nodes != null)
                {
                    foreach (XmlNode Item in nodes)
                    {
                        if (Item.SelectSingleNode("user")?.InnerText == user)
                        {
                            CartCount++;
                        }
                    }
                }
            }

            // Load products from XML database
            var xmlDoc = new XmlDocument();
            xmlDoc.Load("./database/products.xml");
            var products = new List<Product>();
            var nodes_ = xmlDoc?.SelectNodes("/products/product");
            if (nodes_ != null)
            {
                foreach (XmlNode node in nodes_)
                {
                    var product = new Product
                    {
                        Id = Convert.ToInt32(node.SelectSingleNode("id")?.InnerText),
                        Title = node.SelectSingleNode("title")?.InnerText,
                        Image = node.SelectSingleNode("image")?.InnerText,
                        Price = Convert.ToDecimal(node.SelectSingleNode("price")?.InnerText),
                        Description = node.SelectSingleNode("description")?.InnerText,
                    };
                    products.Add(product);
                }
            }

            // Pass data to view
            ViewData["CartCount"] = CartCount;
            ViewData["User"] = user;
            return View(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
