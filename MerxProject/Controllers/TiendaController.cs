using MerxProject.Models;
using MerxProject.Models.CarritoCompras;
using MerxProject.Models.Cupones;
using MerxProject.Models.DetallePedidos;
using MerxProject.Models.Direccion;
using MerxProject.Models.Order;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{
    public class TiendaController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        ApplicationDbContext DbModel;

        public TiendaController()
        {
            this.DbModel = new ApplicationDbContext();
        }

        public TiendaController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }


        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: Tienda
        [Authorize(Roles = "Cliente")]
        public ActionResult IndexTienda(int? outStock, string pagoRealizado = null)
        {
           
            if (pagoRealizado == "True")
            {
                ViewBag.pagoRealizado = "True";
            }
            if (outStock != null)
            {
                ViewBag.outStock = 1;
            }
            else
            {
                ViewBag.outStock = 0;
            }

            using (this.DbModel = new ApplicationDbContext())
            {
                var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
                var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == idPersona.idPersona).ToList();
                ViewBag.cantidadCarrito = cantidadCarrito.Count;


                var model = new CarritoComprasViewModel
                {
                    CarritoCollection = DbModel.CarritoCompras.Where(x => x.idPersona == idPersona.idPersona).OrderBy(x => x.Nombre).ToList()
                };

                model.CantidadesProductos = new List<CarritoCompra>();
                int j = 0;


                var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
                var cupons = DbModel.Cupon.Where(x => x.IdPersona == persona.idPersona).OrderByDescending(x => x.idOrder).ToList();

                if (cupons.Count > 0)
                {
                    int idOrdens = cupons[0].idOrder.Value;
                    var validarCuponOrder = DbModel.Orders.FirstOrDefault(x => x.IdOrder == idOrdens);
                    if (validarCuponOrder.Estatus == "Procesando" || validarCuponOrder.NombreCustomer == "Nueva")
                    {
                        ViewBag.descuentoGuardado = cupons[0].Descuento / 100.00;
                    }
                }


                return View(model);
            }
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<ActionResult> IndexTienda(string cupon, string ship, int subtotal)
        {
            var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
            var ordenesCompra = DbModel.Orders.FirstOrDefault(x => x.idPersona == persona.idPersona && x.Estatus == "Procesando");

            var cupons = DbModel.Cupon.FirstOrDefault(x => x.CodigoCupon == cupon);

            if (cupons.Utilizado)
            {
                ViewBag.CupNotExit = 2;
                IndexTienda(null);
                return View();
            }

            if (cupons != null)
            {
                double descuento = (cupons.Descuento / 100.00);
                descuento *= subtotal;

                var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);

                using (this.DbModel = new ApplicationDbContext())
                {
                    // En caso de que haya generado uan Orden sin cupo pero luego regreso para asignarle un decuento a la compra, procedemos a editar el Orden Actual.
                    if (ordenesCompra != null)
                    {

                        cupons.IdPersona = idPersona.idPersona;
                        cupons.Utilizado = true;
                        cupons.idOrder = ordenesCompra.IdOrder;

                        DbModel.Cupon.AddOrUpdate(cupons);
                        DbModel.SaveChanges();
                    }
                    else
                    {


                        cupons.IdPersona = idPersona.idPersona;
                        cupons.Utilizado = true;

                        MerxProject.Models.Order.Orders order = new Models.Order.Orders()
                        {
                            NombreCustomer = "Nueva",
                            NumeroCustomer = "",
                            DireccionCustomer = "",
                            EmailCustomer = "",
                            idPersona = idPersona.idPersona,
                            DiaOrden = DateTime.Now,
                            TipoPago = "",
                            Estatus = ""

                        };

                        DbModel.Orders.Add(order);
                        DbModel.SaveChanges();


                        cupons.idOrder = order.IdOrder;

                        DbModel.Cupon.AddOrUpdate(cupons);
                        DbModel.SaveChanges();
                    }

                }

                ViewBag.radioBtn = ship;
                ViewBag.subTotalDesc = subtotal - descuento;
                IndexTienda(null);
                return View();
            }
            else
            {
                ViewBag.CupNotExit = 1;
                IndexTienda(null);
                return View();
            }



        }

        [Authorize(Roles ="Cliente")]
        [HttpGet]
        public async Task<ActionResult> PagoProducto(int? dire, string subtotal, string total, string cupon, string ship = "0")
        {
            if (string.IsNullOrEmpty(ship))
            {
                ship = "Gratis";
            }


            using (this.DbModel = new ApplicationDbContext())
            {

                var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
                var ordenPendiente = DbModel.Orders.FirstOrDefault(x => x.idPersona == persona.idPersona && x.Estatus == "Procesando");
                var fromDatabaseEF = new SelectList(DbModel.Direcciones.Where(x => x.IdPersona == persona.idPersona).ToList(), "Id", "DirCalle");
                var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == persona.idPersona).ToList();
                ViewBag.cantidadCarrito = cantidadCarrito.Count;
                double ships = 0;

                if (ship != "Gratis")
                {

                    char[] shipAux = ship.ToCharArray();
                    ship = "";
                    for (int i = 0; i < shipAux.Length; i++)
                    {
                        if (shipAux[i] != '$')
                        {
                            ship += shipAux[i];
                        }

                    }

                    if (ordenPendiente != null)
                    {
                        var dir = DbModel.Direcciones.FirstOrDefault(x => x.Id == ordenPendiente.idDireccion);
                        ships = Convert.ToDouble(ship);
                        ViewBag.MySkills = fromDatabaseEF;
                        ViewBag.subtotal = subtotal;
                        ViewBag.ordenPendiente = 1;
                        ViewBag.total = (Convert.ToDouble(total) + Convert.ToDouble(ships));
                        ViewBag.cupon = cupon;
                        ViewBag.ship = ship;
                        return View(dir);
                    }
                    else
                    {
                        //Significa que no hay orden o esta con estatos "Nueva"
                        ViewBag.ordenPendiente = 0;
                    }


                    if (dire > 0)
                    {
                        var dir = DbModel.Direcciones.FirstOrDefault(x => x.Id == dire);
                        ships = Convert.ToDouble(ship);
                        ViewBag.MySkills = fromDatabaseEF;
                        ViewBag.subtotal = subtotal;
                        ViewBag.total = (Convert.ToDouble(total) + Convert.ToDouble(ships));
                        ViewBag.cupon = cupon;
                        ViewBag.ship = ship;
                        return View(dir);

                    }

                    ships = Convert.ToDouble(ship);
                    ViewBag.MySkills = fromDatabaseEF;
                    ViewBag.subtotal = subtotal;
                    ViewBag.total = (Convert.ToDouble(total) + Convert.ToDouble(ships));
                    ViewBag.cupon = cupon;
                    ViewBag.ship = ship;
                }
                else
                {
                    ViewBag.MySkills = fromDatabaseEF;
                    ViewBag.subtotal = subtotal;
                    ViewBag.total = total;
                    ViewBag.cupon = cupon;
                    ViewBag.ship = ship;
                }



                return View();


            }

        }

        [HttpPost]
        public async Task<ActionResult> PagoProducto(Direcciones direccion, string subtotal, string total, string cupon, string ship)
        {
            if(direccion.Id == 0)
            {
                return RedirectToAction("PagoProducto",new { subtotal = subtotal, total = subtotal, cupon = cupon, ship = ship });
            }
       
            
            using (this.DbModel = new ApplicationDbContext())
            {
                var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
                var direccionDettalle = DbModel.Direcciones.FirstOrDefault(x => x.Id == direccion.Id);

                var fromDatabaseEF = new SelectList(DbModel.Direcciones.Where(x => x.IdPersona == persona.idPersona).ToList(), "Id", "DirCalle");
                var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == persona.idPersona).ToList();
                ViewBag.cantidadCarrito = cantidadCarrito.Count;


                ViewBag.direccion = direccionDettalle.Id;
                ViewBag.MySkills = fromDatabaseEF;
                ViewBag.subtotal = subtotal;
                ViewBag.total = total;
                ViewBag.ordenPendiente = 1;
                ViewBag.cupon = cupon;
                ViewBag.ship = ship;

                ProcessOrder(direccion.Id);

                return View(direccionDettalle);
            }

        }


        public void ProcessOrder(int direccion)
        {

            var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);

            // Cuando llega "Nueva" significa que se utilizó un cupón en la orden
            var orderFind = DbModel.Orders.FirstOrDefault(x => x.idPersona == persona.idPersona && x.NombreCustomer == "Nueva");

            var datosCustomer = DbModel.Direcciones.FirstOrDefault(x => x.Id == direccion);
            int idOrder = 0;

            // Todo el siguiente bloque es usado para generar un detalle de la orden, dando a conoce productos y precios que el usaurio comprara
            using (this.DbModel = new ApplicationDbContext())
            {

                var model = new CarritoComprasViewModel
                {
                    CarritoCollection = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).ToList(),
                };

                model.CantidadesProductos = new List<CarritoCompra>();
                int j = 0, i = 0;

                ////return RedirectToAction("PagoProducto", new { dire = direccion, subtotal = subtotal, total = total, cupon = cupon, ship = ship }) ;



                // Si llega diferente a null significa que se va actualizar el orden que ya existe gracias al cupon
                if (orderFind != null)
                {
                    idOrder = orderFind.IdOrder;


                    MerxProject.Models.Order.Orders order = new Models.Order.Orders()
                    {
                        IdOrder = orderFind.IdOrder,
                        NombreCustomer = datosCustomer.NombreCompleto,
                        NumeroCustomer = datosCustomer.NumTelefono,
                        DireccionCustomer = datosCustomer.DirCalle + ", " + datosCustomer.CodigoPostal + ", " + datosCustomer.Estado + ", " + datosCustomer.Ciudad + ", " + datosCustomer.Asentamiento,
                        EmailCustomer = User.Identity.Name,
                        idPersona = persona.idPersona,
                        idDireccion = datosCustomer.Id,
                        DiaOrden = DateTime.Now,
                        TipoPago = "Pago Online",
                        Estatus = "Procesando"
                    };

                    DbModel.Orders.AddOrUpdate(order);
                    DbModel.SaveChanges();


                }
                else
                {
                    var orderUpdate = DbModel.Orders.FirstOrDefault(x => x.idPersona == persona.idPersona && x.Estatus == "Procesando");



                    if (orderUpdate != null)
                    {

                        MerxProject.Models.Order.Orders order = new Models.Order.Orders()
                        {
                            IdOrder = orderUpdate.IdOrder,
                            NombreCustomer = datosCustomer.NombreCompleto,
                            NumeroCustomer = datosCustomer.NumTelefono,
                            DireccionCustomer = datosCustomer.DirCalle + ", " + datosCustomer.CodigoPostal + ", " + datosCustomer.Estado + ", " + datosCustomer.Ciudad + ", " + datosCustomer.Asentamiento,
                            EmailCustomer = User.Identity.Name,
                            idPersona = persona.idPersona,
                            DiaOrden = DateTime.Now,
                            idDireccion = datosCustomer.Id,
                            TipoPago = "Pago Online",
                            Estatus = "Procesando"
                        };

                        DbModel.Orders.AddOrUpdate(order);
                        DbModel.SaveChanges();
                        idOrder = orderUpdate.IdOrder;
                    }
                    else
                    {

                        MerxProject.Models.Order.Orders order = new Models.Order.Orders()
                        {
                            NombreCustomer = datosCustomer.NombreCompleto,
                            NumeroCustomer = datosCustomer.NumTelefono,
                            DireccionCustomer = datosCustomer.DirCalle + ", " + datosCustomer.CodigoPostal + ", " + datosCustomer.Estado + ", " + datosCustomer.Ciudad + ", " + datosCustomer.Asentamiento,
                            EmailCustomer = User.Identity.Name,
                            idPersona = persona.idPersona,
                            DiaOrden = DateTime.Now,
                            idDireccion = datosCustomer.Id,
                            TipoPago = "Pago Online",
                            Estatus = "Procesando"
                        };

                        DbModel.Orders.Add(order);
                        DbModel.SaveChanges();
                        idOrder = order.IdOrder;




                    }




                }


                var orderDetails = DbModel.OrdersDetails.Where(x => x.idOrder == idOrder).ToList();

                foreach (var item in orderDetails)
                {
                    DbModel.OrdersDetails.Remove(item);
                    DbModel.SaveChanges();
                }


                i = 0;
                foreach (var item in model.CarritoCollection)
                {
                    OrdersDetails ordersDetails = new OrdersDetails()
                    {
                        idOrder = idOrder,
                        idProducto = item.idProducto,
                        idColor = item.idColor,
                        Cantidad = item.Cantidad,
                        Precio = item.Precio
                    };
                    i++;

                    DbModel.OrdersDetails.Add(ordersDetails);
                    DbModel.SaveChanges();
                }
            }
        }

        private Payment payment;


        public async Task<ActionResult> PagoConPaypal(double? totalPagoPyapal, string totalship, string Cancel = null)
        {
            //getting the apiContext
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            var persona = DbModel.Personas.First(x => x.Correo == User.Identity.Name);
            try
            {
                if (totalship == "Gratis")
                {
                    totalship = "0";
                }
                string payerId = Request.Params["PayerID"];
                totalPagoPyapal -= Convert.ToInt32(totalship);
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Tienda/PagoConPaypal?";
                    var guid = Convert.ToString((new Random()).Next(100000000));
                    var CrearPago = this.CrearPago(apiContext, baseURI + "guid=" + guid, totalPagoPyapal, totalship);

                    var links = CrearPago.links.GetEnumerator();
                    string paypalRedirectURL = string.Empty;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectURL = link.href;
                        }
                    }

                    Session.Add(guid, CrearPago.id);
                    return Redirect(paypalRedirectURL);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("Failure");
                    }
                }
            }
            catch (Exception ex)
            {

                PaypalLogger.Log("Error" + ex.Message);
                return View("Failure");
            }
            var orderId = DbModel.Orders.First(x => x.idPersona == persona.idPersona && x.Estatus == "Procesando");
            var orderDetails = DbModel.OrdersDetails.Where(x => x.idOrder == orderId.IdOrder).OrderBy(x => x.idColor).ToList();
            var productos = DbModel.Productos.ToList();
            var colores = DbModel.Colores.ToList();
            var productosList = DbModel.Productos.ToList();
            var inventarioDetail = DbModel.Inventarios.OrderBy(x => x.Producto.Id).ToList();
           

            var carritoBorrar = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).ToList();


            Guid Newguid = Guid.NewGuid();

            for (int i = 0; i < carritoBorrar.Count; i++)
            {
                int idInventario = 0;
                for (int j = 1; j <= carritoBorrar[i].Cantidad; j++)
                {
                    int idProducto = carritoBorrar[i].idProducto;
                    int idColor = carritoBorrar[i].idColor;
                    int idMaterial = carritoBorrar[i].idMaterial;

                    Proceso procesoCrear = new Proceso();
                    procesoCrear.Inventario = new Inventario();
                    procesoCrear.Order = new Orders();
                    Inventario inventario = new Inventario();
                    inventario.Color = new Color();
                    inventario.Material = new Material();
                    inventario.Producto = new Producto();

                    if (carritoBorrar[i].idMaterial > 0 && carritoBorrar[i].idColor > 0)
                    {
                        var ExistInv = DbModel.Inventarios.Where(x => x.Id == idInventario).ToList();

                        if (ExistInv.Count == 0)
                        {

                            inventario.Color = DbModel.Colores.FirstOrDefault(x => x.Id == idColor);
                            inventario.Producto = DbModel.Productos.FirstOrDefault(x => x.Id == idProducto);
                            inventario.Material = DbModel.Materiales.FirstOrDefault(x => x.Id == idMaterial);
                            inventario.Cantidad = 0;

                            DbModel.Inventarios.Add(inventario);
                            DbModel.SaveChanges();

                            idInventario = inventario.Id;

                        }
                     
                    }
                    else
                    {
                        idInventario = DbModel.Inventarios.FirstOrDefault(x => x.Producto.Id == idProducto && x.Color.Id == idColor).Id;
                    }

                    procesoCrear.Id = Guid.NewGuid().ToString();
                    procesoCrear.Nombre = "Corte";
                    procesoCrear.Estado = "En proceso";
                    procesoCrear.Tiempo = DateTime.Now;
                    procesoCrear.Registro = DateTime.Now;
                    procesoCrear.Inventario = DbModel.Inventarios.FirstOrDefault(x => x.Id == idInventario);
                    procesoCrear.Order = orderId;
                    DbModel.Procesos.Add(procesoCrear);
                    DbModel.SaveChanges();

                    DetallePedido detallePedido = new DetallePedido();
                    detallePedido.IdPedidoDetalle = Newguid.ToString();
                    detallePedido.IdProceso = procesoCrear.Id;

                    DbModel.DetallePedidos.Add(detallePedido);
                    DbModel.SaveChanges();

                }
            }

           

            foreach (var item in carritoBorrar)
            {
                DbModel.CarritoCompras.Remove(item);
                DbModel.SaveChanges();
            }

            var orderPgado = DbModel.Orders.FirstOrDefault(x => x.idPersona == persona.idPersona && x.Estatus == "Procesando");

            orderPgado.Estatus = "Pagado";
            DbModel.Orders.AddOrUpdate(orderPgado);
            DbModel.SaveChanges();



            //string code = await UserManager.GenerateEmailConfirmationTokenAsync(User.Identity.GetUserId());
            var callbackUrl = Url.Action("ConsultarPedido", "Proceso", new { Id = Newguid.ToString() }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(User.Identity.GetUserId(), "Pedido", callbackUrl);

            return RedirectToAction("IndexTienda", new { pagoRealizado = "True"});
        }

        private Payment CrearPago(APIContext apiContext, string redirectUrl, double? totalPagoPyapal, string totalship = "0")
        {
           
            var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
            //create itemlist and add item objects to it
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };

            var model = new CarritoComprasViewModel
            {
                CarritoCollection = DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).ToList()
            };

            model.CantidadesProductos = new List<CarritoCompra>();
            int j = 0, i = 0;

            var cuponUsado = DbModel.Cupon.Where(x => x.IdPersona == persona.idPersona).OrderByDescending(x => x.idOrder).ToList();
            var order = DbModel.Orders.FirstOrDefault(x => x.idPersona == persona.idPersona && x.Estatus == "Procesando");
            double descuento = 0.00;
            if (cuponUsado.Count > 0)
            {

                if (cuponUsado[0].idOrder == order.IdOrder)
                {
                    descuento = cuponUsado[0].Descuento / 100.00;

                    foreach (var item in model.CarritoCollection)
                    {
                        double DescAns = Convert.ToDouble(item.Precio - (Convert.ToDouble(item.Precio) * descuento));
                        itemList.items.Add(new Item()
                        {
                            quantity = item.Cantidad.ToString(),
                            name = item.Nombre,
                            price = DescAns.ToString(),
                            currency = "MXN",
                        });
                        i++;
                    }
                }
                else
                {
                    foreach (var item in model.CarritoCollection)
                    {
                        itemList.items.Add(new Item()
                        {
                            // quantity = model.CantidadesProductos[i].Cantidad.ToString(),
                            quantity = item.Cantidad.ToString(),
                            name = item.Nombre,
                            price = item.Precio.ToString(),
                            currency = "MXN",
                        });
                        i++;
                    }
                }
            }
            else
            {
                foreach (var item in model.CarritoCollection)
                {
                    itemList.items.Add(new Item()
                    {
                        quantity = item.Cantidad.ToString(),
                        name = item.Nombre,
                        price = item.Precio.ToString(),
                        currency = "MXN",
                    });
                    i++;
                }
            }



            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details
            var details = new Details()
            {
                tax = "1",
                shipping = totalship,
                subtotal = totalPagoPyapal.ToString()
            };
            //Final amount with details
            var amount = new Amount()
            {
                currency = "MXN",
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(), // Total must be equal to sum of tax, shipping and subtotal.
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "Detalle Transacción",
                invoice_number = "#" + Convert.ToString((new Random()).Next(100000000)), //Generate an Invoice No
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);

        }



        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }



    }

}
