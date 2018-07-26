using MusicStore.Models.Data;
using MusicStore.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MusicStore.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //declare a list of models
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //init the list
                categoryVMList = db.Categories
                                .ToArray()
                                .OrderBy(x => x.Sorting)
                                .Select(x => new CategoryVM(x))
                                .ToList();

            }
                //return view with list


                return View(categoryVMList);
        }
        // POST: Admin/Shop/addCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //declare id
            string id;

            using (Db db = new Db())
            {
                //check category name is unique
                if(db.Categories.Any(x => x.Name == catName))
                   return "titletaken";

                //init DTO
                CategoryDTO dto = new CategoryDTO();

                //add to DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //save DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                //get id
                id = dto.Id.ToString();
            }

            //return id
            return id;
        }
        //POST: admin/shop/reordercategory
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1;

                //declare pages dto
                CategoryDTO dto;

                //set sorting for each page
                foreach (var CatId in id)
                {
                    dto = db.Categories.Find(CatId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }
        }
        // GET: Admin/shop/delecategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //get page
                CategoryDTO dto = db.Categories.Find(id);

                //remove the page
                db.Categories.Remove(dto);

                //save
                db.SaveChanges();
            }

            //redirect


            return RedirectToAction("Categories");
        }
        // POST: Admin/shop/renamecategory/id
        [HttpPost]
        public string RenameCategory( string newCatName, int id )
        {
            using (Db db = new Db())
            {
                // check cat name is unique
                if(db.Categories.Any(x => x.Name == newCatName))
                {
                    return "titletaken";
                }

                //get dto
                CategoryDTO dto = db.Categories.Find(id);

                //edit dto
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ","-").ToLower();

                //save
                db.SaveChanges();
            }
            //return
            return "ok";
        }
        //GET: admin/shop/AddProduct
        public ActionResult AddProduct()
        {
            //init model
            ProductVm model = new ProductVm();

            //add select list of categories to model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id","Name");
            }


                //return view with model
                return View(model);
        }
    }
}