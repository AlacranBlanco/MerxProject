using MerxProject.Models;
using MerxProject.Models.ProductosFavorito;
using MerxProject.Models.TiendaViewModels;
using MerxProject.Models.VentaViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{

    public class ProductoController : Controller
    {
        /*private readonly int _RegistrosPorPagina = 10;

        private List<Inventario> _Productos;
        private PaginadorGenerico<Inventario> _PaginadorProductos;
        private List<Producto> _Producto;
        private PaginadorGenerico<Producto> _PaginadorProducto;

        // GET: Producto
        public ActionResult Tienda(int pagina = 1)*/

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
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Productos.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Productos = DbModel.Inventarios.Where(x=> x.Cantidad > 0)
                                                 .OrderBy(x => x.Producto.Nombre)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProductos = new PaginadorGenerico<Inventario>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Productos
                };
                // Enviamos a la Vista la 'Clase de paginación'
                return View(_PaginadorProductos);
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
                var producto = DbModel.Inventarios.Find(id);
                return View(producto);
            }
        }

        public ActionResult Buscar(string parameter, int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;

                // Número total de registros de la tabla Productos donde sean parecidos al parámetro
                _TotalRegistros = DbModel.Inventarios.Where(x => x.Producto.Nombre.Contains(parameter) ||
                    (x.Producto.Descripcion.Contains(parameter)) ||
                    (x.Color.Nombre.Contains(parameter)) ||
                    (x.Producto.CategoriaMaterial.Nombre.Contains(parameter)) ||
                    (x.Producto.CategoriaMueble.Nombre.Contains(parameter)) ||
                    (x.Producto.Precio.ToString().Contains(parameter))).Count();
                // Obtenemos la 'página de registros' de la tabla Productos donde sean parecidos al parámetro
                _Productos = DbModel.Inventarios.Where(x => x.Producto.Nombre.Contains(parameter) ||
                    (x.Producto.Descripcion.Contains(parameter)) ||
                    (x.Color.Nombre.Contains(parameter)) ||
                    (x.Producto.CategoriaMaterial.Nombre.Contains(parameter)) ||
                    (x.Producto.CategoriaMueble.Nombre.Contains(parameter)) ||
                    (x.Producto.Precio.ToString().Contains(parameter))).OrderBy(x => x.Producto.Nombre)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos donde sean parecidos al parámetro
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProductos = new PaginadorGenerico<Inventario>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Productos
                };

                if (_TotalRegistros < 1)
                {
                    ViewBag.alert = ("No hay resultados");
                    return View("Tienda");
                }
                else
                {
                    return View(_PaginadorProductos);
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

                            if(postedFile != null)
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
                            }
                            else
                            {
                                productos.Imagen = producto.Imagen;
                            }

                            try
                            {
                                
                                DbModel.Productos.AddOrUpdate(productos);
                                DbModel.SaveChanges();
                                resultado = "Actualización realizada";
                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaProducto");

                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                Session["res"]= resultado;
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        else
                        {
                            resultado = "Error";
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        resultado = "Error";
                        Session["res"] = resultado;
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
                            DbModel.Productos.Remove(producto);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProducto");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        resultado = "Error";
                        Session["res"] = resultado;
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
                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaProducto");
                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                Session["res"] = resultado;
                                return RedirectToAction("ListaProducto");
                            }
                        }
                        else
                        {
                            resultado = "Error";
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    resultado = "Error";
                    Session["res"] = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaProducto");


            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpProductosColor(int Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                Inventario inv = new Inventario();
                
                var Colores = DbModel.Colores.ToList();
                var ColoresHechos = new List<Inventario>();
                var producto = DbModel.Productos.Find(Id);
                var inventarios = DbModel.Inventarios.ToList();
                var invId = new List<Inventario>();

                foreach(var item in inventarios)
                {
                    if (item.Producto != null)
                    {
                        if (producto.Id == item.Producto.Id)
                        {
                            invId.Add(item);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                foreach(var item in invId)
                {
                    foreach(var color in Colores)
                    {
                        if(item.Color.Id == color.Id)
                        {
                            ColoresHechos.Add(item);
                        }
                    }
                }

                foreach(var item in ColoresHechos)
                {
                    Colores.Remove(item.Color);
                }

                int suma = 0;
                foreach (var item in invId)
                {
                    suma += item.Cantidad;
                }
                

                ViewBag.Colores = Colores;
                ViewBag.productoId = producto.Id;
                ViewBag.invId = invId;
                ViewBag.total = suma;

                ViewBag.title = "Inventario";
                ViewBag.subtitle = producto.Nombre;
                

                return View(inv);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpProductosColor(int productoId, string cantidad, string newColor, string radio, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                Inventario inv = new Inventario();


                string resultado;
                try
                {
                    if (accion == "Editar")
                    {
                        if (radio != null)
                        {
                            inv = DbModel.Inventarios.Find(Convert.ToInt16(radio));
                            inv.Cantidad = Convert.ToInt16(cantidad);

                            DbModel.Inventarios.AddOrUpdate(inv);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProducto");
                        }
                        else if (newColor != null && radio == null)
                        {
                            var productos = DbModel.Productos.Find(productoId);
                            inv.Producto = productos;

                            Color colour = new Color();
                            colour = DbModel.Colores.Find(Convert.ToInt16(newColor));
                            inv.Color = colour;

                            inv.Cantidad = Convert.ToInt16(cantidad);

                            DbModel.Inventarios.Add(inv);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProducto");
                        }
                    }
                    else
                    {
                        inv = DbModel.Inventarios.Find(Convert.ToInt16(radio));
                        DbModel.Inventarios.Remove(inv);
                        DbModel.SaveChanges();
                        resultado = "Eliminación realizada";
                        Session["res"] = resultado;
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaProducto");
                    }
                }
                catch (Exception ex)
                {
                    resultado = ex.Message;
                    Session["res"] = resultado;
                    return RedirectToAction("ListaProducto");
                }
            }
            return RedirectToAction("ListaProducto");
        }

        #region Braulio Monroy

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaProducto(int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                var Colores = DbModel.Colores.ToList();

                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;
                ViewBag.Colores = Colores;



                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Productos.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Producto = DbModel.Productos.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProducto = new PaginadorGenerico<Producto>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Producto
                };
                
             
                return View(_PaginadorProducto);
            }
        }

        public ActionResult BuscarLista(string orden, string categoria, string parameter, int pagina = 1)
        {
           
            int _TotalRegistros = 0;
            var propertyInfo = typeof(Producto).GetProperty(parameter);
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Materiales = DbModel.Materiales.ToList();
                var Muebles = DbModel.Muebles.ToList();
                ViewBag.Materiales = Materiales;
                ViewBag.Muebles = Muebles;

                if (string.IsNullOrWhiteSpace(categoria) && string.IsNullOrWhiteSpace(orden) && string.IsNullOrWhiteSpace(parameter))
                {
                    _TotalRegistros = DbModel.Productos.Count();
                    // Obtenemos la 'página de registros' de la tabla Productos
                    _Producto = DbModel.Productos.OrderBy(x => x.Nombre)
                                                     .Skip((pagina - 1) * _RegistrosPorPagina)
                                                     .Take(_RegistrosPorPagina)
                                                     .ToList();
                    // Número total de páginas de la tabla Productos
                    var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                    // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                    _PaginadorProducto = new PaginadorGenerico<Producto>()
                    {
                        RegistrosPorPagina = _RegistrosPorPagina,
                        TotalRegistros = _TotalRegistros,
                        TotalPaginas = _TotalPaginas,
                        PaginaActual = pagina,
                        Resultado = _Producto
                    };
                    return View("ListaProducto", _PaginadorProductos);
                }
                else if(!string.IsNullOrWhiteSpace(categoria) && string.IsNullOrWhiteSpace(orden))
                {
                    orden = "Asc";
                }
                else if(string.IsNullOrWhiteSpace(categoria) && !string.IsNullOrWhiteSpace(orden))
                {
                    categoria = "Nombre";
                }

                switch (categoria)
                {
                    case "Precio":
                        if (string.IsNullOrWhiteSpace(parameter))
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x=> x.Precio).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderByDescending(x => x.Precio)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.Precio).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderBy(x => x.Precio)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                        }
                        else
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.Precio)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderByDescending(x => x.Precio)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderBy(x => x.Precio)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderBy(x => x.Precio)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }

                            if (_TotalRegistros < 1)
                            {
                                ViewBag.error = "No hay resultados";
                                return View("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProducto);

                    case "Nombre":
                        if (string.IsNullOrWhiteSpace(parameter))
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderByDescending(x => x.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderBy(x => x.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                        }
                        else
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderByDescending(x => x.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderBy(x => x.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderBy(x => x.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }

                            if (_TotalRegistros < 1)
                            {
                                ViewBag.error = "No hay resultados";
                                return View("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProductos);

                    case "CatMaterial":
                        if (string.IsNullOrWhiteSpace(parameter))
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.CategoriaMaterial.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderByDescending(x => x.CategoriaMaterial.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.CategoriaMaterial.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderBy(x => x.CategoriaMaterial.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                        }
                        else
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.CategoriaMaterial.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderByDescending(x => x.CategoriaMaterial.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderBy(x => x.CategoriaMaterial.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderBy(x => x.CategoriaMaterial.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }

                            if (_TotalRegistros < 1)
                            {
                                ViewBag.error = "No hay resultados";
                                return View("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProductos);

                    case "CatMueble":
                        if (string.IsNullOrWhiteSpace(parameter))
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.CategoriaMueble.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderByDescending(x => x.CategoriaMueble.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.OrderBy(x => x.CategoriaMueble.Nombre).Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.OrderBy(x => x.CategoriaMueble.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                        }
                        else
                        {
                            if (orden == "Desc")
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderByDescending(x => x.CategoriaMueble.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderByDescending(x => x.CategoriaMueble.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }
                            else
                            {
                                // Número total de registros de la tabla Productos
                                _TotalRegistros = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                    (x.Descripcion.Contains(parameter)) ||
                                                                    (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                    (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                    (x.Precio.ToString().Contains(parameter)))
                                                                    .OrderBy(x => x.CategoriaMueble.Nombre)
                                                                    .Count();
                                // Obtenemos la 'página de registros' de la tabla Productos
                                _Producto = DbModel.Productos.Where(x => x.Nombre.Contains(parameter) ||
                                                                 (x.Descripcion.Contains(parameter)) ||
                                                                 (x.CategoriaMaterial.Nombre.Contains(parameter)) ||
                                                                 (x.CategoriaMueble.Nombre.Contains(parameter)) ||
                                                                 (x.Precio.ToString().Contains(parameter)))
                                                                 .OrderBy(x => x.CategoriaMueble.Nombre)
                                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                 .Take(_RegistrosPorPagina)
                                                                 .ToList();
                                // Número total de páginas de la tabla Productos
                                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                                _PaginadorProducto = new PaginadorGenerico<Producto>()
                                {
                                    RegistrosPorPagina = _RegistrosPorPagina,
                                    TotalRegistros = _TotalRegistros,
                                    TotalPaginas = _TotalPaginas,
                                    PaginaActual = pagina,
                                    Resultado = _Producto
                                };
                            }

                            if (_TotalRegistros < 1)
                            {
                                ViewBag.error = "No hay resultados";
                                return View("ListaProducto");
                            }
                        }
                        return View("ListaProducto", _PaginadorProductos);
                        
                    default:
                        ViewBag.error = "No hay resultados";
                        _TotalRegistros = DbModel.Productos.Count();
                        // Obtenemos la 'página de registros' de la tabla Productos
                        _Producto = DbModel.Productos.OrderBy(x => x.Nombre)
                                                         .Skip((pagina - 1) * _RegistrosPorPagina)
                                                         .Take(_RegistrosPorPagina)
                                                         .ToList();
                        // Número total de páginas de la tabla Productos
                        var _TP = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                        // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                        _PaginadorProducto = new PaginadorGenerico<Producto>()
                        {
                            RegistrosPorPagina = _RegistrosPorPagina,
                            TotalRegistros = _TotalRegistros,
                            TotalPaginas = _TP,
                            PaginaActual = pagina,
                            Resultado = _Producto
                        };
                        return View("ListaProducto", _PaginadorProductos);
                }
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