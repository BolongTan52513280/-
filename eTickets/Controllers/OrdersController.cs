using Amazoom.Data;
using Amazoom.Data.Cart;
using Amazoom.Data.Services;
using Amazoom.Data.Static;
using Amazoom.Data.ViewModels;
using Amazoom.IPC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Amazoom.Controllers
{
    [Authorize] 
    public class OrdersController : Controller
    {//constructor
        private readonly IItemsService _ItemsService;
        private readonly ShoppingCart _shoppingCart;
        private readonly IOrdersService _ordersService;
        private readonly AppDbContext _context;



        public OrdersController(IItemsService ItemsService, ShoppingCart shoppingCart, IOrdersService ordersService)
        {
            _ItemsService = ItemsService;
            _shoppingCart = shoppingCart;
            _ordersService = ordersService;
        }

        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userRole = User.FindFirstValue(ClaimTypes.Role);//admin or user

            var orders = await _ordersService.GetOrdersByUserIdAndRoleAsync(userId, userRole);
            return View(orders);
        }

        public IActionResult ShoppingCart()
        {//get the added items
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;
         //get the response of the shoppingcart view according to the statement
            var response = new ShoppingCartVM()
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal()
            };
            //display the items in the cart
            return View(response);
        }

        public async Task<IActionResult> AddItemToShoppingCart(int id)
        {
            var item = await _ItemsService.GetItemByIdAsync(id);

            if (item != null)
            {
                _shoppingCart.AddItemToCart(item);
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        public async Task<IActionResult> RemoveItemFromShoppingCart(int id)
        {
            var item = await _ItemsService.GetItemByIdAsync(id);

            if (item != null)
            {
                _shoppingCart.RemoveItemFromCart(item);
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        public async Task<IActionResult> CompleteOrder()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userEmailAddress = User.FindFirstValue(ClaimTypes.Email);

            foreach (var ShoppingCartItem in items) {
                await _ItemsService.UpdateStockAsync(ShoppingCartItem.Item.Id , ShoppingCartItem.Amount);
                    }
            await _ordersService.StoreOrderAsync(items, userId, userEmailAddress);
            await _shoppingCart.ClearShoppingCartAsync();


            //make the orders into a string
            string OrderInfo = "0";
            foreach (var ShoppingCartItem in items)
            {
                for(int i = 0; i < ShoppingCartItem.Amount; i++)
                {
                    OrderInfo += ",".ToString();
                    OrderInfo += ShoppingCartItem.Item.Id.ToString();
                }
            }

            foreach (var ShoppingCartItem in items)
            {
                if ((ShoppingCartItem.Item.stock - ShoppingCartItem.Amount) < 0)
                {
                    return View("Orderfailed");
                }
            }



                //test the string on MVC side
                Console.WriteLine(OrderInfo);


            //flush the string through the pipe to the console app
            PipeClient.Client(OrderInfo);            

            //return completed notification view
            return View("OrderCompleted");
        }
    }
}
