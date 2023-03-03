using Amazoom.Data;
using Amazoom.Data.Services;
using Amazoom.Data.Static;
using Amazoom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amazoom.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class ItemsController : Controller
    {
        private readonly IItemsService _service;
        /*functions:
        Task<Item> GetItemByIdAsync(int id);
        Task AddNewItemAsync(NewItemVM data);
        Task UpdateItemAsync(NewItemVM data);*/

        public ItemsController(IItemsService service)
        {
            _service = service;
        }

        [AllowAnonymous]//Can be viewed without signing in
        public async Task<IActionResult> Index()
        {//Show the index that contains the view of the whole page
            var allItems = await _service.GetAllAsync();
            return View(allItems);
        }
        //Search bar
        [AllowAnonymous]//Can be viewed without signing in
        public async Task<IActionResult> Filter(string searchString)
        {
            var allItems = await _service.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                //var filteredResult = allItems.Where(n => n.Name.ToLower().Contains(searchString.ToLower()) || n.Description.ToLower().Contains(searchString.ToLower())).ToList();

                var filteredResultNew = allItems.Where(n => string.Equals(n.Name, searchString, StringComparison.CurrentCultureIgnoreCase) || string.Equals(n.Description, searchString, StringComparison.CurrentCultureIgnoreCase)).ToList();

                return View("Index", filteredResultNew);
            }

            return View("Index", allItems);
        }
        
        //GET: Items/Details/1
        [AllowAnonymous]
        //item detail page (show the description of the item correspoding to the item id
        public async Task<IActionResult> Details(int id)
        {
            var ItemDetail = await _service.GetItemByIdAsync(id);
            return View(ItemDetail);
        }

        //The edit page
        //GET: Items/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var ItemDetails = await _service.GetItemByIdAsync(id);
            if (ItemDetails == null) return View("NotFound");
            //Change the info of the items 
            var response = new NewItemVM()
            {
                Id = ItemDetails.Id,
                Name = ItemDetails.Name,
                Description = ItemDetails.Description,
                Price = ItemDetails.Price,
                weight = ItemDetails.weight,
                volume = ItemDetails.volume,
                stock = ItemDetails.stock,
                ImageURL = ItemDetails.ImageURL,
            };

            return View(response);
        }
        
    }
}
