using MerxProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{
    public class ColorController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Color> _Colores;
        private PaginadorGenerico<Color> _PaginadorColores;

        [HttpGet]
        public ActionResult popUpColores(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (accion == "1")
                {
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    ViewBag.img = false;
                    return View();
                }
                else if (accion == "2")
                {
                    var color = DbModel.Colores.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.Codigo = color.Codigo;
                    return View(color);
                }
                else if (accion == "3")
                {
                    var color = DbModel.Colores.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    return View(color);
                }
            }
            return RedirectToAction("ListaColor");
        }

        [HttpPost]
        public async Task<ActionResult> popUpColores(Color color, string accion, string Codigo)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (color.Id > 0 && accion == "2")
                {
                    // Edición
                    var Color = DbModel.Colores.Find(color.Id);
                    if(Codigo != Color.Codigo)
                    {
                        color.Codigo = Codigo;
                    }

                    if (Color != null)
                    {
                        try
                        {
                            DbModel.Colores.AddOrUpdate(color);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaColor");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaColor");
                        }
                    }

                }
                else if (color.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var Color = DbModel.Colores.Find(color.Id);

                    if (Color != null)
                    {

                        try
                        {
                            var inv = DbModel.Inventarios.Where(x => x.Color.Id == Color.Id).ToList();
                            if (inv.Count() < 1)
                            {
                                if(Color.Nombre == "Natural")
                                {
                                    Session["res"] = "El color natural no se puede eliminar";
                                    return RedirectToAction("ListaColor");
                                }
                                DbModel.Colores.Remove(Color);
                                var Pintura = DbModel.Materiales.Where(x => x.Nombre == "Pintura " + Color.Nombre).FirstOrDefault();
                                if(Pintura != null)
                                {
                                    DbModel.Materiales.Remove(Pintura);
                                }
                                DbModel.SaveChanges();
                                resultado = "Eliminación finalizada";
                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaColor");
                            }
                            else
                            {
                                resultado = "Aún hay productos con este color, primero se tienen que quitar";
                                Session["res"] = resultado;
                                return RedirectToAction("ListaColor");
                            }
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaColor");
                        }
                    }

                }
                else
                {
                    if (color != null)
                    {
                        // Aquí código para crear
                        try
                        {
                            color.Codigo = Codigo;

                            DbModel.Colores.Add(color);

                            var ColorMaterial = new Material()
                            {
                                Nombre = "Pintura " + color.Nombre,
                                Cantidad = 0,
                                Descripcion = "Pintura para el color " + color.Nombre,
                                Medida = "Litros",
                                Piezas = false,
                                Precio = 0,
                                Imagen = null
                            };
                            DbModel.Materiales.Add(ColorMaterial);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaColor");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaColor");
                        }
                    }
                    resultado = "Error";
                    Session["res"] = resultado;
                }

                return RedirectToAction("ListaMaterial");


            }
        }

        [Authorize(Roles = "Administrador, Empleado")]
        [HttpGet]
        public ActionResult ListaColor(int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if(DbModel.Colores.Where(x=>x.Nombre == "Natural").Count() < 1)
                {
                    var NColor = new Color()
                    {
                        Nombre = "Natural",
                        Codigo = "#ffffff",
                    };

                    DbModel.Colores.Add(NColor);
                }
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Colores.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Colores = DbModel.Colores.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorColores = new PaginadorGenerico<Color>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Colores
                };
                return View(_PaginadorColores);
            }
        }
    }
}