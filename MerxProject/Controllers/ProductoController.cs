using MerxProject.Models;
using MerxProject.Models.ProductosFavorito;
using MerxProject.Models.TiendaViewModels;
using MerxProject.Models.VentaViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{
    public class ProductoController : Controller
    {

        ApplicationDbContext DbModel;


        public ProductoController()
        {
            this.DbModel = new ApplicationDbContext();
        }

        // GET: Producto
        [HttpGet]
        public ActionResult Tienda(int? NoLogIn, int pag = 1)
        {
            if (pag <= 0) pag = 1;
            if (NoLogIn != null) ViewBag.NoLogIn = 0;
            var email = User.Identity.Name;
            var usuarioLoggeado = DbModel.Personas.FirstOrDefault(x => x.Correo == email);
            

            TiendaViewModel tiendaViewModel = new TiendaViewModel();
            tiendaViewModel.ProductoCollection = DbModel.Productos.ToList();

            tiendaViewModel.ProductosFavoritosColelction = DbModel.ProductosFavoritos.Where(x => x.idPersona == usuarioLoggeado.idPersona).OrderBy(x => x.idProducto).ToList();

            tiendaViewModel.ProductosFavId = new List<int>();

            int idxAux = 0;
            foreach (var item in tiendaViewModel.ProductoCollection)
            {
                if (idxAux < tiendaViewModel.ProductosFavoritosColelction.Count)
                {
                    if (item.Id == tiendaViewModel.ProductosFavoritosColelction[idxAux].idProducto)
                    {
                        tiendaViewModel.ProductosFavId.Add(item.Id);
                        idxAux++;
                    }
                    else
                    {
                        tiendaViewModel.ProductosFavId.Add(-12);
                    }
                }
                else
                {
                    tiendaViewModel.ProductosFavId.Add(-12);
                }
                
            }


                ViewBag.pagina = pag;
                return View(tiendaViewModel);
           
            


         
        }

        [HttpPost]
        public ActionResult Tienda()
        {
            
            return View();
        }
        public ActionResult MostrarTodos()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                //Obtiene todos los productos y los manda a listado
                var productos = DbModel.Productos.ToList();
                return PartialView("_MostrarTodos", productos);
            }
        }

        public ActionResult VistaProducto(int? id)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                //Obtiene el producto que coincida con el ID seleccionado de la lista MostrarTodos(). 
                var producto = DbModel.Productos.Find(id);
                return View(producto);
            }
        }

        public ActionResult Buscar(string parameter)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                //Searh es un método de NinjaNye.SearchExtensions: Colección de métodos a IQueryable y IEnumerable que facilitan las búsquedas. 

                //SearchExtension
                //var productos1 = DbModel.Productos.Search(x => x.Nombre,
                //    x => x.Descripcion, x => x.Color, x => "" + x.Precio).
                //    Containing(parameter).ToList();

                var productos2 = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                    (x.Descripcion.Contains(parameter)) ||
                    (x.Color.Contains(parameter)) ||
                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                    (x.Precio.ToString().Contains(parameter))).ToList();

                if (productos2.Count() < 1)
                {
                    ViewBag.alert = ("No hay resultados");
                    return View("Store");
                }
                else
                {

                    return View(productos2);
                }
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpProductos(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                if (accion == "1")
                {
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    ViewBag.img = false;
                    return View();
                }
                else if (accion == "2")
                {
                    var producto = DbModel.Productos.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(producto);
                }
                else if (accion == "3")
                {
                    var producto = DbModel.Productos.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(producto);
                }
            }
            return RedirectToAction("ListaMueble");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpProductos(Producto productos, int idMueble, int idMaterial, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (productos.Id > 0 && accion == "2")
                {
                    // Edición
                    var producto = DbModel.Productos.Find(productos.Id);
                    if (producto != null)   
                    {

                        var material = DbModel.Materiales.Find(idMaterial);
                        var mueble = DbModel.Muebles.Find(idMueble);

                        if (material != null && mueble != null)
                        {

                            productos.CategoriaMaterial = material;
                            productos.CategoriaMueble = mueble;

                            try
                            {
                                
                                DbModel.Productos.AddOrUpdate(productos);
                                DbModel.SaveChanges();
                                resultado = "Actualización realizada";
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaProducto");

                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        else
                        {
                            resultado = "Error";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        resultado = "Error";
                        ViewBag.res = resultado;
                        return RedirectToAction("ListaProducto");
                    }

                }
                else if (productos.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var producto = DbModel.Productos.Find(productos.Id);

                    if (producto != null)
                    {

                        try
                        {
                            DbModel.Productos.Remove(productos);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        resultado = "Error";
                        ViewBag.res = resultado;
                        return RedirectToAction("ListaProducto");
                    }

                }
                else
                {
                    string dir = Server.MapPath("~/Content/assets/img/Productos");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var originalFile = Path.GetFileName(postedFile.FileName);
                    string fileId = Guid.NewGuid().ToString().Replace("-", "");
                    var path = Path.Combine(dir, fileId);
                    postedFile.SaveAs(path + postedFile.FileName);
                    productos.Imagen = fileId + postedFile.FileName;


                    if (productos != null)
                    {
                        var material = DbModel.Materiales.Find(idMaterial);
                        var mueble = DbModel.Muebles.Find(idMueble);

                        if (material != null && mueble != null)
                        {

                            productos.CategoriaMaterial = material;
                            productos.CategoriaMueble = mueble;
                            // Aquí código para crear
                            try
                            {

                                DbModel.Productos.Add(productos);
                                DbModel.SaveChanges();
                                resultado = "Inserción realizada";
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaProducto");
                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        else
                        {
                            resultado = "Error";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaProducto");


            }
        }


        #region Braulio Monroy

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaProducto()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;

                var productos = DbModel.Productos.ToList();
                return View(productos);
            }
        }

      

        [AllowAnonymous]
        [HttpGet]
        public ActionResult  ShelveProduct1()
        {
            return View("");
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult Bamboo1(string name)
        {

            if(name != null)
            {
                VentaViewModels ventaViewModels = new VentaViewModels();

                ventaViewModels.Producto = DbModel.Productos.FirstOrDefault(x => x.Nombre == name);
                ventaViewModels.InventarioCollection = DbModel.Inventarios.Where(x => x.Producto_Id == ventaViewModels.Producto.Id).ToList();
                ventaViewModels.ColorsCollection = new List<Colors>();
                foreach (var item in ventaViewModels.InventarioCollection)
                {
                    var colorDetail = DbModel.Colors.FirstOrDefault(x => x.Id == item.Color_Id);
                    ventaViewModels.ColorsCollection.Add(colorDetail);
                }
            return View(ventaViewModels);
            }
            else
            {
                return Redirect("Tienda");
            }
           
            

        }

      
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AgregarProductoFavorito(Producto producto, int accion, int pagi)
        {
            var correo = User.Identity.Name;
            if (correo != "")
            {
                if (accion == 1)
                {
                    ProductosFavoritos productosFavoritos = new ProductosFavoritos();
                    using (this.DbModel = new ApplicationDbContext())
                    {

                        var idPersonas = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
                        var nombreProdcuto = DbModel.Productos.FirstOrDefault(x => x.Id == producto.Id);
                        productosFavoritos.Nombre = nombreProdcuto.Nombre;
                        productosFavoritos.idProducto = producto.Id;
                        productosFavoritos.idPersona = idPersonas.idPersona;
                        DbModel.ProductosFavoritos.Add(productosFavoritos);
                        DbModel.SaveChanges();
                    }
                    return RedirectToAction("Tienda", new { pag = pagi });
                }
                else if (accion == 2)
                {
                    // Eliminación
                    var productoFav = DbModel.ProductosFavoritos.FirstOrDefault(x => x.idProducto == producto.Id);

                    if (productoFav != null)
                    {
                        DbModel.ProductosFavoritos.Remove(productoFav);
                        DbModel.SaveChanges();
                        return RedirectToAction("Tienda", new { pag = pagi });
                    }
                }
            }
            else
            {
                // El usuario no ha sido logeado, le mandamos un mensaje
                ViewBag.NoLogIn = 0;
                ViewBag.pagina = 1;
                Tienda(0, 1);
                return View("Tienda");
            }
           
        

            return View("Tienda");

        }







        #endregion

    }
}