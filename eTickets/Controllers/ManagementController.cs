using Amazoom.Data;
using Amazoom.IPC;
using Amazoom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Amazoom.Controllers
{
    //[Route("product")]
    public class ManagementController : Controller
    {
        private AppDbContext db;

        public ManagementController(AppDbContext _db)
        {
            db = _db;
        }


        public IActionResult Index()
        {
            ViewBag.items = db.Items.ToList();
            return View();
        }



        


            /* 
             * Written by Daniel Wu @2021-12-1
             * Here we use the ActionResult and return RedirectionToAction method to remian the view we want
             * If we don't do this, the webapp will redirect to an empty page called RemoveProductFromStock page automatically
             * 
             * 
            */
            public ActionResult RemoveProductFromStock(Item eachItem)
        {
            var itemInStock = db.Items.FirstOrDefault(n => n.Id == eachItem.Id);

            if (itemInStock != null)
            {
                if (itemInStock.stock > 1)
                {
                    itemInStock.stock--;
                }
                else
                {
                    db.Items.Remove(itemInStock);
                }
            }
            db.SaveChanges();






            //make the orders into a string
            string OrderInfo = "0";
            OrderInfo += itemInStock.Id.ToString();
            //test the string on MVC side
            Console.WriteLine(OrderInfo);
            //flush the string through the pipe to the console app
            PipeClient.Client(OrderInfo);




            return RedirectToAction("Index");
        }


        public ActionResult AddProductFromStock(Item eachItem)
        {
            var itemInStock = db.Items.FirstOrDefault(n => n.Id == eachItem.Id);

            itemInStock.stock++;
            db.SaveChanges();



            //make the orders into a string
            string OrderInfo = "1,";
            OrderInfo += itemInStock.Id.ToString();
            //test the string on MVC side
            Console.WriteLine(OrderInfo);
            //flush the string through the pipe to the console app
            PipeClient.Client(OrderInfo);





            return RedirectToAction("Index");
        }





        /*
        [Route("edit")]
        [HttpGet]
        public IActionResult Edit()
        {
            return View("Edit");
        }

        [Route("edit")]
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        */
    }
}