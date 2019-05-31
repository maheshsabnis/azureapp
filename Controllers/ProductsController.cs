using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MVC_CosmosDB.Models;
using MVC_CosmosDB.Services;

namespace MVC_CosmosDB.Controllers
{
    public class ProductsController : Controller
    {
        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await ProductsService.GetOpenProductsAsync();
            return View(items);
        }

        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            var product = new ProductInfo();
            return View(product);
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync(ProductInfo product)
        {
            if (ModelState.IsValid)
            {
                await ProductsService.CreateProductAsync(product);
                return RedirectToAction("Index");
            }

            return View(product);
        }


        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id, string categoryName)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductInfo productToUpdate = await ProductsService.GetProductInfoAsync(id, categoryName);
            if (productToUpdate == null)
            {
                return HttpNotFound();
            }

            return View(productToUpdate);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProductAsync(ProductInfo product)
        {
            if (ModelState.IsValid)
            {
                await ProductsService.UpdateProductAsync(product);
                return RedirectToAction("Index");
            }

            return View(product);
        }

        

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id, string categoryName)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductInfo productToDelete = await ProductsService.GetProductInfoAsync(id, categoryName);
            if (productToDelete == null)
            {
                return HttpNotFound();
            }

            return View(productToDelete);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteProductAsync(ProductInfo product)
        {
            await ProductsService.DeleteProductAsync(product.Id, product.CategoryName);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id, string categoryName)
        {
            ProductInfo productDetails = await ProductsService.GetProductInfoAsync(id,  categoryName);
            return View(productDetails);
        }
    }
}