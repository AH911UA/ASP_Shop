using ShopEnergy.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;


namespace ShopEnergy.Controllers
{
    public class HomeController : Controller
    {
        ShopContext shopContext = new ShopContext();

        [HttpGet]
        public ActionResult Index()
        {
            return View(shopContext.Products);
        }


        [HttpGet]
        public ActionResult Basket()
        {
            List<Product> productsBasket = new List<Product>();
            int? idPerson = shopContext.People.Where(per => per.Login == User.Identity.Name).FirstOrDefault().Id;
            if(idPerson != null)
            {
                List<Order> idProducts = shopContext.Orders.Where(ord => ord.PersonId == idPerson).ToList();
                if (idProducts.Count != 0)  
                    foreach (var item in idProducts)
                    {
                        Product tmpProd = shopContext.Products.FirstOrDefault(e => e.Id == item.ProductId);
                        if (tmpProd != null)
                            productsBasket.Add(tmpProd);
                    }
                return View(productsBasket);
            }
            //foreach (var person in shopContext.People)
            //{
            //    if (person.Login == User.Identity.Name)
            //    {
            //        foreach (var order in shopContext.Orders)
            //        {
            //            if (order.PersonId == person.Id)
            //            {
            //                p.Add(new Product
            //                {

            //                });
            //            }
            //        }
            //    }
            //}

            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        [HttpPost]
        public ActionResult Index(Product p)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToRoute(new { controller = "Home", action = "Index" });

            foreach (var item in shopContext.People)
                if (item.Login == User.Identity.Name)
                {
                    shopContext.Orders.Add(new Order
                    {
                        PersonId = item.Id,
                        ProductId = p.Id
                    });
                    break;
                }

            shopContext.SaveChanges();
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult EditProductRedact(Product p)
        {
            HttpPostedFileBase file = null;
            string fileName = "";
            Product pr = new Product();

            try
            {
                file = Request.Files[0];
                if (file.FileName != "")
                {
                    fileName = System.IO.Path.GetFileName(file.FileName);

                    if (!fileName.EndsWith(".jpg") | !fileName.EndsWith(".png"))
                    {
                        TempData["msg"] = "<script>alert('Неверный формат файла');</script>";
                        return RedirectToRoute(new { controller = "Home", action = "Edit" });
                    }

                    if (Directory.GetFiles(Server.MapPath("~/Content/ProductImg/")).Contains(fileName))
                    {
                        TempData["msg"] = "<script>alert('Изображение с таким именем уже существует');</script>";
                        return RedirectToRoute(new { controller = "Home", action = "Edit" });
                    }
                    file.SaveAs(Server.MapPath("~/Content/ProductImg/" + fileName));
                    pr.DescriptionImg = file.FileName;
                }

                pr.Id = p.Id;
                pr.Name = p.Name;
                pr.Price = p.Price;
                pr.DescriptionText = p.DescriptionText;


                foreach (var item in shopContext.Products)
                {
                    if (item.Id == p.Id)
                    {
                        item.Name = pr.Name;
                        item.Price = pr.Price;
                        item.DescriptionText = p.DescriptionText;

                        if (pr.DescriptionImg != null)
                            item.DescriptionImg = pr.DescriptionImg;

                        break;
                    }
                }

                shopContext.SaveChanges();
            }
            catch
            {
                TempData["msg"] = "<script>alert('Ошибка при загрузке');</script>";
            }

            return RedirectToRoute(new { controller = "Home", action = "Edit" });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult Edit(Product p)
        {
            HttpPostedFileBase file = null;
            string fileName = "";

            try
            {
                file = Request.Files[0];
                if (file != null)
                {
                    fileName = System.IO.Path.GetFileName(file.FileName);

                    if (fileName.EndsWith(".jpg") | !fileName.EndsWith(".png"))
                    {
                        TempData["msg"] = "<script>alert('Неверный формат файла');</script>";
                        return View(shopContext.Products);
                    }

                    if (Directory.GetFiles(Server.MapPath("~/Content/ProductImg/")).Contains(fileName))
                    {
                        TempData["msg"] = "<script>alert('Изображение с таким именем уже существует');</script>";
                        return View(shopContext.Products);
                    }
                    file.SaveAs(Server.MapPath("~/Content/ProductImg/" + fileName));

                    shopContext.Products.Add(new Product
                    {
                        Name = p.Name,
                        Price = p.Price,
                        DescriptionImg = fileName,
                        DescriptionText = p.DescriptionText
                    });

                    shopContext.SaveChanges();
                }
            }
            catch
            {
                TempData["msg"] = "<script>alert('Ошибка при загрузке');</script>";
            }

            return View(shopContext.Products);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult EditProductRemove(int? id)
        {
            var product = shopContext.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                bool isRedact = false;
                foreach (var item in shopContext.Products)
                {
                    if (item.Id == id)
                    {
                        string fileName = System.IO.Path.GetFileName(item.DescriptionImg);
                        System.IO.File.Delete(Server.MapPath("~/Content/ProductImg/") + fileName);

                        shopContext.Products.Remove(item);
                        isRedact = true;
                        break;
                    }
                }

                if (isRedact)
                    shopContext.SaveChanges();
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            return HttpNotFound();
        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult EditProduct(int? id)
        {
            var product = shopContext.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
                return View(product);
            return HttpNotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult Edit() => View(shopContext.Products);

        void AddProduct()
        {
            ShopContext shopContext = new ShopContext();
            bool isEmpty = true;
            foreach (var item in shopContext.People)
            {
                if (item.Login == User.Identity.Name)
                {
                    isEmpty = false;
                    break;
                }
                else
                    isEmpty = true;
            }

            if (isEmpty)
                shopContext.People.Add(new Person { Login = User.Identity.Name });
        }
        protected override void Dispose(bool disposing)
        {
            shopContext.Dispose();

            base.Dispose(disposing);
        }


        public ActionResult RemoveProduct(Product p)
        {
            bool isRemove = false;
            foreach (var item in shopContext.People)
                if (item.Login == User.Identity.Name)
                {
                    Order o = shopContext.Orders.Where(or => (or.PersonId == item.Id && or.ProductId == p.Id)).FirstOrDefault();
                    if (o != null)
                    {
                        shopContext.Orders.Remove(o);
                        isRemove = true;
                    }
                    break;
                }

            if(isRemove)
                shopContext.SaveChanges();

            return RedirectToRoute(new { controller = "Home", action = "Basket" });
        }

        public ActionResult MakeOrder()
        {
            bool isRemove = false;
            foreach (var item in shopContext.People)
                if (item.Login == User.Identity.Name)
                {
                    Order[] o = shopContext.Orders.Where(or => (or.PersonId == item.Id)).ToArray();
                    if (o.Length != 0)
                    {
                        shopContext.Orders.RemoveRange(o);
                        isRemove = true;
                    }
                    break;
                }

            string request = "<script>alert('Произошла ошибка, сорян бро');</script>";
            if (isRemove)
            {
                shopContext.SaveChanges();
                request = "<script>alert('Заказ сделан, ждите звоночка');</script>";
            }

            TempData["makeOrder"] = request;
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }






    }
}