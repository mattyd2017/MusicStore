using MusicStore.Models.Data;
using MusicStore.Models.ViewModels.Shop;
using PagedList;
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
        //GET: admin/shop/Products

        public ActionResult Products(int? page, int? catId)
        {
            //declare a list of productVM
            List<ProductVm> listOfProductVm;
            //set page number
            var pagenumber = page ?? 1;

            using (Db db = new Db())
            {
                //init the list
                listOfProductVm = db.Products.ToArray()
                                   .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                   .Select(x => new ProductVm(x))
                                   .ToList();
                //populate cat select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //set selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            //set pagination
            var onePageOfProducts = listOfProductVm.ToPagedList(pagenumber, 5);
            ViewBag.OnePageOfProducts = onePageOfProducts;
            //return view list


            return View(listOfProductVm);
        }
        //GET: admin/shop/EditProducts/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //declare productvm
            ProductVm model;

            using (Db db = new Db())
            {
                //get product
                ProductDTO dto = db.Products.Find(id);
                //confirm product exists
                if(dto == null)
                {
                    return Content("The product does not exist");
                }

                //init model
                model = new ProductVm(dto);

                //make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                .Select(fn => Path.GetFileName(fn));
            }
                //return product view with model

                return View(model);
        }
        //POST: admin/shop/EditProducts/id
        [HttpPost]
        public ActionResult EditProduct (ProductVm model,HttpPostedFileBase file)
        {
            //get product id
            int id = model.Id;

            //populate categories select list and gallery images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));

            //check model state
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            //make sure product is unique
            using (Db db = new Db())
            {
                if(db.Products.Where(x => x.Id != id ).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }
            }
            //update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Description = model.Description;
                dto.Slug = model.Name.Replace(" ","-").ToLower();
                dto.Price = model.Price;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;
                db.SaveChanges();

            }

            //set tempdata message
            TempData["SM"] = "You have successfully edited the product";

            #region Image Upload


                //check for file upload
                if (file != null && file.ContentLength > 0)
                {
                    //get extension
                    string ext = file.ContentType.ToLower();
                    //verify file extension
                    if (ext != "image/jpg" &&
                       ext != "image/jpeg" &&
                       ext != "image/pjpeg" &&
                       ext != "image/gif" &&
                       ext != "image/png" &&
                       ext != "image/x-png")
                    {
                        using (Db db = new Db())
                        {


                            ModelState.AddModelError("", "The image was not uploaded to server - wrong image format");
                            return View(model);

                        }

                    }

                //set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //delete files from directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();
                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();

                //save image name
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //save original and thumb images
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(300, 300);
                img.Save(path2);
            }


            #endregion

            //redirect
            return RedirectToAction("EditProduct");
        }
        //GET: admin/shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //delete product from db
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }
            //delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString,true);

            //redirect
            return RedirectToAction("Products");
        }
        //POST: admin/shop/savegalleryimages
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //loop through the files
            foreach (string fileName in Request.Files)
            {
                //init the file
                HttpPostedFileBase file = Request.Files[fileName];

                //check its not null
                if (file != null && file.ContentLength > 0)
                {
                    //set directory paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");
                    //set image path
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //save orginal and thumbs
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(300, 300);
                    img.Save(path2);

                }
            }

        }
        //POST: admin/shop/deletegalleryimages
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);
            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);

        }


    }
}