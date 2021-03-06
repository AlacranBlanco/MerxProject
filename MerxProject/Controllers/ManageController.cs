﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MerxProject.Models;
using Newtonsoft.Json;
using System.Net;
using System.Data.Entity.Migrations;
using MerxProject.Models.Direccion;
using System.IO;

namespace MerxProject.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext DbModel;

        public ManageController()
        {
            this.DbModel = new ApplicationDbContext();
        }






        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
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

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Su contraseña se ha cambiado."
                : message == ManageMessageId.SetPasswordSuccess ? "Su contraseña se ha establecido."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Su proveedor de autenticación de dos factores se ha establecido."
                : message == ManageMessageId.Error ? "Se ha producido un error."
                : message == ManageMessageId.AddPhoneSuccess ? "Se ha agregado su número de teléfono."
                : message == ManageMessageId.RemovePhoneSuccess ? "Se ha quitado su número de teléfono."
                : "";

            var userId = User.Identity.GetUserId();
            var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == User.Identity.Name);
            var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == idPersona.idPersona).ToList();
            ViewBag.cantidadCarrito = cantidadCarrito.Count;
            ViewBag.userEmail = User.Identity.Name;
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generar el token y enviarlo
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Su código de seguridad es: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Enviar un SMS a través del proveedor de SMS para verificar el número de teléfono
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // Si llegamos a este punto, es que se ha producido un error, volvemos a mostrar el formulario
            ModelState.AddModelError("", "No se ha podido comprobar el teléfono");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            var correo = User.Identity.Name;
            var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
            var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == idPersona.idPersona).ToList();
            ViewBag.cantidadCarrito = cantidadCarrito.Count;
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var correo = User.Identity.Name;
            var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
            var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == idPersona.idPersona).ToList();
            ViewBag.cantidadCarrito = cantidadCarrito.Count;

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            var gmail = User.Identity.GetUserName();
            using (this.DbModel = new ApplicationDbContext())
            {
                Usuario usuario = new Usuario();
                usuario = DbModel.Usuarios.FirstOrDefault(x => x.User == gmail);
                usuario.Password = model.NewPassword;
                DbModel.Usuarios.AddOrUpdate(usuario);
                DbModel.SaveChanges();

            }
            

            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            var correo = User.Identity.Name;
            var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
            var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == idPersona.idPersona).ToList();
            ViewBag.cantidadCarrito = cantidadCarrito.Count;
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {

            var correo = User.Identity.Name;
            var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
            var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == idPersona.idPersona).ToList();
            ViewBag.cantidadCarrito = cantidadCarrito.Count;
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

                var gmail = User.Identity.GetUserName();
                using (this.DbModel = new ApplicationDbContext())
                {
                    Usuario usuario = new Usuario();
                    usuario = DbModel.Usuarios.FirstOrDefault(x => x.User == gmail);
                    usuario.Password = model.NewPassword;
                    DbModel.Usuarios.AddOrUpdate(usuario);
                    DbModel.SaveChanges();

                }

                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // Si llegamos a este punto, es que se ha producido un error, volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "Se ha quitado el inicio de sesión externo."
                : message == ManageMessageId.Error ? "Se ha producido un error."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Solicitar la redirección al proveedor de inicio de sesión externo para vincular un inicio de sesión para el usuario actual
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }


        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public ActionResult Direcciones(int? id, int? accion)
        {
            if (id == null && accion == null)
            {
                return View();
            }
            if (accion.Value == 2)
            {
                var direccion = DbModel.Direcciones.Find(id.Value);
                ViewBag.accion = accion;
                return View(direccion);
            }
            else if (accion.Value == 1)
            {
                ViewBag.accion = accion;
                Direcciones direcciones = new Direcciones();
                return View(direcciones);
            }
            else
            {
                return RedirectToAction("MisDirecciones");

            }

        }
        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<ActionResult> Direcciones(Direcciones dir, int accion)
        {

            if (accion == 1)
            {
                using (this.DbModel = new ApplicationDbContext())
                {
                    var correo = User.Identity.Name;
                    var idPersonas = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
                    dir.IdPersona = idPersonas.idPersona;
                    DbModel.Direcciones.Add(dir);
                    DbModel.SaveChanges();
                }
                return RedirectToAction("MisDirecciones");
            }
            else if (accion == 2)
            {
                using (this.DbModel = new ApplicationDbContext())
                {
                    DbModel.Direcciones.AddOrUpdate(dir);
                    DbModel.SaveChanges();

                }
                return RedirectToAction("MisDirecciones");
            }
            return RedirectToAction("MisDirecciones");
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public ActionResult MisDirecciones()
        {
            var correo = User.Identity.Name;
            var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
            var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == idPersona.idPersona).ToList();
            ViewBag.cantidadCarrito = cantidadCarrito.Count;
            var MisDirecciones = DbModel.Direcciones.Where(x => x.IdPersona == idPersona.idPersona).ToList();
            return View(MisDirecciones);

        }

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<ActionResult> MisDirecciones(int? id, int accion)
        {
            try
            {
                var correo = User.Identity.Name;
                var idPersona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
                var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == idPersona.idPersona).ToList();
                ViewBag.cantidadCarrito = cantidadCarrito.Count;

                if (accion == 3)
                {
                    // Eliminación
                    var direccion = DbModel.Direcciones.Find(id);

                    if (direccion != null)
                    {
                        DbModel.Direcciones.Remove(direccion);
                        DbModel.SaveChanges();
                        return RedirectToAction("MisDirecciones");
                    }
                }
            }

            catch (Exception ex)
            {

                return View();
            }

            return View();

        }

        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public async Task<ActionResult> ChangeEmail(string email, string isChanged = null, string emailExist = null)
        {
            var persona = DbModel.Personas.FirstOrDefault(x => x.Correo == email);
            var model = UserManager.FindByEmail(email);
            var carritoExist =  DbModel.CarritoCompras.Where(x => x.idPersona == persona.idPersona).ToList();
            ViewBag.cantidadCarrito = carritoExist.Count;
            ViewBag.carritoExist = carritoExist.Count();
            if (isChanged == "1"){ ViewBag.isChanged = 1; }
            if (emailExist == "1") { ViewBag.emailExist = 1; }
            return View(model);

        }

        [HttpPost]
        public async Task<ActionResult> ChangeEmail(ApplicationUser applicationUser, string emailNuevo)
        {
            var result = DbModel.Personas.FirstOrDefault(x => x.Correo == emailNuevo);

            if (result != null)
            {
               
                return RedirectToAction("ChangeEmail", new { email = applicationUser.Email, isChanged = "", emailExist = "1" });
            }
            else
            {
                var getUser = UserManager.FindByEmail(applicationUser.Email);
                var personaUpd = DbModel.Personas.FirstOrDefault(x => x.Correo == getUser.Email);
                var cantidadCarrito = DbModel.CarritoCompras.Where(X => X.idPersona == personaUpd.idPersona).ToList();
                ViewBag.cantidadCarrito = cantidadCarrito.Count;
                getUser.Email = emailNuevo;
                getUser.UserName = emailNuevo;
                getUser.EmailConfirmed = false;

                var userMana = UserManager.Update(getUser);
                personaUpd.Correo =  emailNuevo;

                DbModel.Personas.AddOrUpdate(personaUpd);
                DbModel.SaveChanges();

                string code = await UserManager.GenerateEmailConfirmationTokenAsync(getUser.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = getUser.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(getUser.Id, "Confirmar cuenta", callbackUrl);

                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index", "Home");

                // return RedirectToAction("ChangeEmail", new { email = emailNuevo, isChanged = "1", emailExist = "" });
            }

          

        }


        #region Aplicaciones auxiliares
        // Se usan para protección XSRF al agregar inicios de sesión externos
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}