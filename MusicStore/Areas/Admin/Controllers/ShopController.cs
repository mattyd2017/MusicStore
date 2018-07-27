using MusicStore.Models.Data;
using MusicStore.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
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
        [HttpGet]
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
        //POST: admin/shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVm model, HttpPostedFileBase file)
        {
            // check model state
            if(!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

            }

            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name already exists please use another");
                    return View(model);
                }
            }

            //declare product id
            int id;

            //init and save productDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                // get the id
                id = product.Id;
            }

            //set tempdate message
            TempData["SM"] = "You have successfully added a product!!";



            #region Upload Image

            //create directories for images
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));


            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //check if file was uploaded

            if (file != null && file.ContentLength > 0)
            {
                //get file extension
                string ext = file.ContentType.ToLower();
                //verify file extension

                if(ext != "image/jpg"&&
                   ext != "image/jpeg"&&
                   ext != "image/pjpeg"&&
                   ext != "image/gif"&&
                   ext != "image/png"&&
                   ext != "image/x-png")
                {
                    using (Db db = new Db())
                    {

                          model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                          ModelState.AddModelError("", "The image was not uploaded to server - wrong image format");
                          return View(model);

                    }

                }

                //init file name
                string imageName = file.FileName;

                //save image name to dto
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //save original image
                file.SaveAs(path);

                //create and save thumb image
                WebImage img = new WebImage(file.InputStream);
                img.Resize(300, 300);
                img.Save(path2);
            }
            #endregion


            //redirect
            return RedirectToAction("AddProduct");
        }
    }
}