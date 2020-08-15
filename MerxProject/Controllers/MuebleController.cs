using MerxProject.Models;
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
    [Authorize(Roles = "Administrador, Empleado")]
    public class MuebleController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Mueble> _Muebles;
        private PaginadorGenerico<Mueble> _PaginadorMuebles;

        [HttpGet]
        public ActionResult popUpMuebles(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Piezas = DbModel.Piezas.ToList();
                
                ViewBag.Piezas = Piezas;
                if (accion == "1")
                {
                    var Mueble = new Mueble();
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    ViewBag.img = false;
                    return View(Mueble);
                }
                else if (accion == "2")
                {

                    var DM = DbModel.DetalleMuebles.Where(x => x.Mueble.Id == Id ).ToList();
                    var material = DbModel.Muebles.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    if(material.Imagen != null)
                    {
                        ViewBag.img = true;
                    }
                    ViewBag.DM = DM;
                    ViewBag.Id = Id;
                    return View(material);
                }
                else if (accion == "3")
                {
                    var DM = DbModel.DetalleMuebles.Where(x => x.Mueble.Id == Id).ToList();
                    var material = DbModel.Muebles.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    if (material.Imagen != null)
                    {
                        ViewBag.img = true;
                    }
                    ViewBag.DM = DM;
                    ViewBag.Id = Id;
                    return View(material);
                }
            }
            return RedirectToAction("ListaMueble");
        }

        [HttpPost]
        public async Task<ActionResult> popUpMuebles(int? IdMueble, Mueble muebles, string accion, HttpPostedFileBase postedFile, int[] Id, int[] Cantidad)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (muebles.Id > 0 && accion == "2")
                {
                    // Edición
                    var mueble = DbModel.Muebles.Find(IdMueble);
                    muebles.Id = mueble.Id;

                    if (mueble != null)
                    {
                        try
                        {
                            if(postedFile != null)
                            {
                                string dir = Server.MapPath("~/Content/assets/img/Muebles");
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }

                                var originalFile = Path.GetFileName(postedFile.FileName);
                                string fileId = Guid.NewGuid().ToString().Replace("-", "");
                                var path = Path.Combine(dir, fileId);
                                postedFile.SaveAs(path + postedFile.FileName);
                                muebles.Imagen = fileId + postedFile.FileName;
                            }
                            else
                            {
                                muebles.Imagen = mueble.Imagen;
                            }
                            DbModel.Muebles.AddOrUpdate(muebles);

                            for (int i = 0; i < Cantidad.Length; i++)
                            {
                                var pieza = DbModel.Piezas.Find(Id[i]);
                                var DMP = DbModel.DetalleMuebles.Where(x => x.Pieza.Id == pieza.Id && x.Mueble.Id == mueble.Id).Count();
                                var DM = new DetalleMueble();

                                if (Cantidad[i] > 0)
                                {
                                    if (DMP > 0)
                                    {
                                        DM = DbModel.DetalleMuebles.Where(x => x.Pieza.Id == pieza.Id && x.Mueble.Id == mueble.Id).First();
                                        DM.Cantidad = Cantidad[i];
                                    }
                                    else
                                    {
                                        DM.Cantidad = Cantidad[i];
                                        DM.Mueble = mueble;
                                        DM.Pieza = pieza;
                                    }
                                    DbModel.DetalleMuebles.AddOrUpdate(DM);
                                    DM = null;
                                }
                                else
                                {
                                    if (DMP > 0)
                                    {
                                        DM = DbModel.DetalleMuebles.Where(x => x.Pieza.Id == pieza.Id && x.Mueble.Id == mueble.Id).First();
                                        DbModel.DetalleMuebles.Remove(DM);
                                    }
                                }
                            }
                            
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaMueble");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }

                }
                else if (muebles.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var mueble = DbModel.Muebles.Find(IdMueble);
                    muebles.Id = mueble.Id;

                    if (mueble != null)
                    {

                        try
                        {
                            var productos = DbModel.Productos.Where(x => x.CategoriaMueble.Id == mueble.Id).Count();
                            if (productos < 1)
                            {
                                var procesosActuales = DbModel.Procesos.Where(x => x.Inventario.Producto.CategoriaMueble.Id == mueble.Id).Count();
                                if (procesosActuales < 1)
                                {
                                    DbModel.Muebles.Remove(mueble);
                                    DbModel.SaveChanges();
                                    resultado = "Eliminación finalizada";
                                    Session["res"] = resultado;
                                    Session["tipo"] = "Exito";
                                    return RedirectToAction("ListaMueble");
                                }
                                else
                                {
                                    resultado = "Un producto de este tipo está en proceso. No se puede eliminar";
                                    Session["res"] = resultado;
                                    return RedirectToAction("ListaMueble");
                                }
                            }
                            else
                            {
                                resultado = "Aún hay productos con este tipo de mueble, primero se tienen que quitar";
                                Session["res"] = resultado;
                                return RedirectToAction("ListaMueble");
                            }
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }

                }
                else
                {
                    string dir = Server.MapPath("~/Content/assets/img/Muebles");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var originalFile = Path.GetFileName(postedFile.FileName);
                    string fileId = Guid.NewGuid().ToString().Replace("-", "");
                    var path = Path.Combine(dir, fileId);
                    postedFile.SaveAs(path + postedFile.FileName);
                    muebles.Imagen = fileId + postedFile.FileName;


                    if (muebles != null)
                    {
                        // Aquí código para crear
                        try
                        {

                            DbModel.Muebles.Add(muebles);

                            var DetalleMueble = new List<DetalleMueble>();
                            for (int i = 0; i < Cantidad.Length; i++)
                            {
                                if (Cantidad[i] > 0)
                                {
                                    var pieza = DbModel.Piezas.Find(Id[i]);

                                    var DM = new DetalleMueble()
                                    {
                                        Cantidad = Cantidad[i],
                                        Mueble = muebles,
                                        Pieza = pieza
                                    };
                                    DetalleMueble.Add(DM);
                                }
                            }
                            DbModel.DetalleMuebles.AddRange(DetalleMueble);

                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaMueble");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }
                    resultado = "Error";
                    Session["res"] = resultado;
                }

                return RedirectToAction("ListaMaterial");


            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador, Empleado")]
        public ActionResult ListaMueble(int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var count = DbModel.Piezas.Count();
                if (count < 1)
                {
                    var Piezas = new List<Pieza>();
                    Pieza pieza1 = new Pieza()
                    {
                        Nombre = "Viga"
                    };
                    Piezas.Add(pieza1);
                    Pieza pieza2 = new Pieza()
                    {
                        Nombre = "Tablón"
                    };
                    Piezas.Add(pieza2);
                    Pieza pieza3 = new Pieza()
                    {
                        Nombre = "Tabla"
                    };
                    Piezas.Add(pieza3);
                    Pieza pieza4 = new Pieza()
                    {
                        Nombre = "Tarima"
                    };
                    Piezas.Add(pieza4);
                    Pieza pieza5 = new Pieza()
                    {
                        Nombre = "Lata"
                    };
                    Piezas.Add(pieza5);
                    Pieza pieza6 = new Pieza()
                    {
                        Nombre = "Regruesos"
                    };
                    Piezas.Add(pieza6);
                    Pieza pieza7 = new Pieza()
                    {
                        Nombre = "Chapas"
                    };
                    Piezas.Add(pieza7);
                    Pieza pieza8 = new Pieza()
                    {
                        Nombre = "Vigueta"
                    };
                    Piezas.Add(pieza8);
                    Pieza pieza9 = new Pieza()
                    {
                        Nombre = "Alfarjía"
                    };
                    Piezas.Add(pieza9);
                    Pieza pieza10 = new Pieza()
                    {
                        Nombre = "Listones"
                    };
                    Piezas.Add(pieza10);
                    Pieza pieza11 = new Pieza()
                    {
                        Nombre = "Listoncillos"
                    };
                    Piezas.Add(pieza11);
                    DbModel.Piezas.AddRange(Piezas);
                    DbModel.SaveChanges();
                }
            
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Muebles.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Muebles = DbModel.Muebles.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorMuebles = new PaginadorGenerico<Mueble>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Muebles
                };
                return View(_PaginadorMuebles);
            }
        }

        public ActionResult BuscarLista(string parameter, int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos donde sean parecidos al parámetro
                _TotalRegistros = DbModel.Muebles.Where(x => x.Nombre.Contains(parameter) ||
                    (x.Descripcion.Contains(parameter))).Count();
                // Obtenemos la 'página de registros' de la tabla Productos donde sean parecidos al parámetro
                _Muebles = DbModel.Muebles.Where(x => x.Nombre.Contains(parameter) ||
                    (x.Descripcion.Contains(parameter))).OrderBy(x => x.Nombre)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos donde sean parecidos al parámetro
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorMuebles = new PaginadorGenerico<Mueble>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Muebles
                };

                if (_TotalRegistros < 1)
                {
                    ViewBag.alert = ("No hay resultados");
                    return RedirectToAction("ListaMueble");
                }
                else
                {
                    return View("ListaMueble", _PaginadorMuebles);
                }
            }
        }
    }
}