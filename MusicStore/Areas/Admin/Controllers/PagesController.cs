using MusicStore.Models.Data;
using MusicStore.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MusicStore.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //declare list of pagevm
            List<PagesVM> pageslist;


            using (Db db = new Db())
            {
                //init the list
                pageslist = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PagesVM(x)).ToList();


            }

            //return view with list

            return View(pageslist);
        }
        // GET: Admin/pages/addPages
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/pages/addPages
        [HttpPost]
        public ActionResult AddPage(PagesVM model)
        {
            //check model state
            if(! ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //declare slug
                string slug;

                //into pageDTO
                PageDTO dto = new PageDTO();

                //DTO title
                dto.Title = model.Title;

                //check and set slug
                if(string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                //check slug and title are unique
                if(db.Pages.Any(x => x.Title== model.Title)|| db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "The title or slug already exists");
                    return View(model);
                }

                //DTO the remaining parameters
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                dto.Sorting = 100;

                //save dto
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //set template message
            TempData["SM"] = "You have successfully created a new page";

            //redirect
            return RedirectToAction("AddPage");

        }

        // GET: Admin/pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //declare pageVM
            PagesVM model;

            using (Db db = new Db())
            {
                //get page
                PageDTO dto = db.Pages.Find(id);
                //confirm page exists
                if(dto == null)
                {
                    return Content("The page is not found");
                }
                //init pageVm
                model = new PagesVM(dto);
            }
            //return view with model
            return View(model);
        }

        // POST: Admin/pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PagesVM model)
        {
            //check model state
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //get page id
                int id = model.Id;

                //declare slug
                string slug = "home";

                //get the page
                PageDTO dto = db.Pages.Find(id);

                //dto the title
                dto.Title = model.Title;

                //check for slug and set if required
                if(model.Slug != "home")
                {
                    if(string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //make sure slug and title are unique
                if(db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title)|| db.Pages.Where(x => x.Id !=id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "the title or slug does not exist!!!");
                    return View(model);
                }

                //dto the rest of the parameters
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;

                //save dto
                db.SaveChanges();
            }

            //set temp date message
            TempData["SM"] = "You have successfully edited the page";

            //redirect
            return RedirectToAction("EditPage");
        }

        // GET: Admin/pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //declare pagevm
            PagesVM model;
            using (Db db = new Db())
            {
                //get the page
                PageDTO dto = db.Pages.Find(id);

                //confirm page exists
                if(dto == null)
                {
                    return Content("Page cannot be found!!!");
                }
                //init pagevm
                model = new PagesVM(dto);
            }


            //return view with model
            return View(model);
        }

        // GET: Admin/pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //get page
                PageDTO dto = db.Pages.Find(id);

                //remove the page
                db.Pages.Remove(dto);

                //save
                db.SaveChanges();
            }

            //redirect


            return RedirectToAction("Index");
        }
        // POST: Admin/pages/ReorderPage
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1;

                //declare pages dto
                PageDTO dto;

                //set sorting for each page
                foreach(var pageId in id )
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }
        }
        // GET: Admin/pages/editsidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //declare model
            SidebarVM model;

            using (Db db = new Db())
            {
                //get dto
                SidebarDTO dto = db.Sidebar.Find(1);

                //init model
                model = new SidebarVM(dto);
            }

                //return view with model
                return View(model);
        }
        // POST: Admin/pages/editsidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //get the dto
                SidebarDTO dto = db.Sidebar.Find(1);

                //Dto the body
                dto.Body = model.Body;

                //save
                db.SaveChanges();
            }
            //set tempdata message
            TempData["SM"] = "The sidebar has been successfully edited";

            //redirect
            return RedirectToAction("EditSidebar");


        }


    }
}