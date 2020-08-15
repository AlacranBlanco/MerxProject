using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MerxProject.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Security.Provider;
using System.Collections.Specialized;
using NinjaNye.SearchExtensions;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MerxProject.Controllers
{


    //[Authorize]
    [AllowAnonymous]
    public class AccountController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ILogger<AccountController> logger;
        ApplicationDbContext DbModel;
        public AccountController()
        {
            this.DbModel = new ApplicationDbContext();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ILogger<AccountController> logger)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            this.logger = logger;
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

        public ILogger<AccountController> Logger { get; }

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
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, int? isEmailVerified)
        {
            if (isEmailVerified == 1)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.isRegister = 1;
                return View();
            }
            else if (isEmailVerified == 2)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.isConfirmed = 2;
                return View();
            }
            else if (isEmailVerified == 3)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.isLoginFailed = 3;
                return View();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult listaWey()
        {
            var users = UserManager.Users;

            return View(users);

        }



        [Authorize(Roles = "Administrador, Empleado")]
        [HttpGet]
        public ActionResult popupUsuarios(string Id, string accion)
        {
            if (accion == "1")
            {
                ViewBag.title = "Nuevo";
                return View();
            }
            else if (accion == "2")
            {
                var user = UserManager.Users.Where(x => x.Id == Id).FirstOrDefault();
                ViewBag.title = "Editar";
                return View(user);
            }
            else if (accion == "3")
            {
                var user = UserManager.Users.Where(x => x.Id == Id).FirstOrDefault();
                ViewBag.title = "Eliminar";
                return View(user);
            }
            return RedirectToAction("listaWey");
        }

        [Authorize(Roles = "Administrador, Empleado")]
        [HttpPost]
        public async Task<ActionResult> popupUsuarios(ApplicationUser applicationUser, string accion)
        {
            // Si todo el formulario fue llenado correctamente
            if (ModelState.IsValid)
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (applicationUser.Id != null && accion == "2")
                {
                    // Edición
                    var user = await UserManager.FindByIdAsync(applicationUser.Id);

                    if (user != null)
                    {
                        // Lo que hacemos aquí es saber si user fue encontrado de alguna forma dentro de la Base
                        // si es así pues continuamos con la edición.
                        user.Email = applicationUser.Email;
                        user.UserName = applicationUser.UserName;
                        // Método async para poder realizar la actualización en base
                        var result = await UserManager.UpdateAsync(user);

                        // Si todo va chido, regresamos a la lista para ver el editado.
                        if (result.Succeeded)
                        {
                            return RedirectToAction("listaWey");
                        }

                    }

                }
                else if (applicationUser.Id != null && accion == "3")
                {
                    // Eliminación
                    var user = await UserManager.FindByIdAsync(applicationUser.Id);

                    // Lo que hacemos aquí es saber si user fue encontrado de alguna forma dentro de la Base
                    // si es así pues continuamos con la edición.

                    // Método async para poder realizar la actualización en base
                    var result = await UserManager.DeleteAsync(user);

                    // Si todo va chido, regresamos a la lista para ver el editado.
                    if (result.Succeeded)
                    {
                        return RedirectToAction("listaWey");
                    }
                }
                else
                {
                    // Aquí código para crear


                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                    return View("listaWey");
                }

            }
            return View("listaWey");
        }


        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, string RememberMe)
        {
            //if (!ModelState.IsValid)
            //{
            //    ModelState.AddModelError("", "¡Vaya, parece que el usuario o contraseña que has escrito no son correctos!");
            //    return View(model);
            //}

            // No cuenta los errores de inicio de sesión para el bloqueo de la cuenta
            // Para permitir que los errores de contraseña desencadenen el bloqueo de la cuenta, cambie a shouldLockout: true
            if (RememberMe == "true")
            {
                model.RememberMe = true;
            }
            else
            {
                model.RememberMe = false;
            }
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            var user = DbModel.Users.Where(x => x.UserName == model.Email).FirstOrDefault();
            var RoleStore = new RoleStore<IdentityRole>(DbModel);
            var RoleMngr = new RoleManager<IdentityRole>(RoleStore);
            var roles = RoleMngr.Roles.ToList();

            // Si queremos diferencias  entre roles tendrémos que hacer un where a persona y luego a usuarios mediante el email
            // El email es unico y no puede repetirse
            bool rolEmpleado = false;
            bool rolAdmin = false;
            if (user != null)
            {
                if (user.Roles.Count() > 0)
                {
                    foreach (var rol in roles)
                    {
                        if (user.Roles.First().RoleId == rol.Id && rol.Name == "Empleado")
                        {
                            rolEmpleado = true;
                        }
                        else if (user.Roles.First().RoleId == rol.Id && rol.Name == "Administrador")
                        {
                            rolAdmin = true;
                        }
                    }
                }

                if (result == SignInStatus.Success)
                {
                    if (user != null && user.EmailConfirmed && rolEmpleado)
                    {
                        Session["user"] = user.UserName;
                        return RedirectToAction("ListaProducto", "Producto");
                    }
                    else if (user != null && user.EmailConfirmed && rolAdmin)
                    {
                        Session["user"] = user.UserName;
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (user != null && user.EmailConfirmed)
                    {
                        Session["user"] = user.UserName;
                        return RedirectToAction("Index", "Home");
                        //return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account", new { isEmailVerified = 3 });
                    }
                }
                else if (result == SignInStatus.LockedOut)
                {
                    return View("Lockout");
                }
                else if (result == SignInStatus.RequiresVerification)
                {
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
            }

            // SignInStatus.Failure:
            ModelState.AddModelError("", "¡Vaya, parece que el usuario o contraseña que has escrito no son correctos!");
            return View(model);


        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Requerir que el usuario haya iniciado sesión con nombre de usuario y contraseña o inicio de sesión externo
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // El código siguiente protege de los ataques por fuerza bruta a los códigos de dos factores. 
            // Si un usuario introduce códigos incorrectos durante un intervalo especificado de tiempo, la cuenta del usuario 
            // se bloqueará durante un período de tiempo especificado. 
            // Puede configurar el bloqueo de la cuenta en IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código no válido.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string agree, string nombre)
        {
            if (agree != "true")
            {
                // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
                ModelState.AddModelError("", "Debe aceptar los terminos y condiciones");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                int idUsuario = 0;
                int idPersona = 0;

                using (this.DbModel = new ApplicationDbContext())
                {
                    Usuario usuario = new Usuario();
                    usuario.User = model.Email;
                    usuario.Password = model.Password;
                    usuario.Rol = "Cliente";
                    DbModel.Usuarios.Add(usuario);
                    DbModel.SaveChanges();
                    idUsuario = usuario.idUsuario;
                    Persona persona = new Persona();
                    persona.Nombre = nombre;
                    persona.Correo = model.Email;
                    persona.idUsuario = idUsuario;
                    DbModel.Personas.Add(persona);
                    DbModel.SaveChanges();
                    idPersona = persona.idPersona;


                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email, idPersona = idPersona, IdUsuario = idUsuario };
                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {


                        // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                        // Enviar correo electrónico con este vínculo
                        var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbModel));

                        if (!RoleManager.RoleExists("Cliente"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Cliente";
                            RoleManager.Create(role);
                        }
                        var rol = RoleManager.FindByName("Cliente").Name;
                        var UsuarioNuevo = UserManager.FindByEmail(model.Email).Id;
                        UserManager.AddToRole(UsuarioNuevo, rol);
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", callbackUrl);
                        return RedirectToAction("Login", "Account", new { isEmailVerified = 1 });


                        // await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        // return RedirectToAction("Index", "Home");

                    }
                    AddErrors(result);
                }

            }
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult EmailVerification() { return View(); }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);

            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account", new { isEmailVerified = 2 });
            }
            else
            {
                return View("Error");
            }
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // No revelar que el usuario no existe o que no está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar correo electrónico con este vínculo
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Restablecer contraseña", "Para restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Solicitar redireccionamiento al proveedor de inicio de sesión externo
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generar el token y enviarlo
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Si el usuario ya tiene un inicio de sesión, iniciar sesión del usuario con este proveedor de inicio de sesión externo
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Si el usuario no tiene ninguna cuenta, solicitar que cree una
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Obtener datos del usuario del proveedor de inicio de sesión externo
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Aplicaciones auxiliares
        // Se usa para la protección XSRF al agregar inicios de sesión externos
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }


        }
        #endregion
    }
}