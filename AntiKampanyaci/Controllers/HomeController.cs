using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook;
using Microsoft.AspNet.Mvc.Facebook.Client;
using AntiKampanyaci.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;

namespace AntiKampanyaci.Controllers
{
    public class HomeController : BaseController
    {


        [FacebookAuthorize()]
        public async Task<ActionResult> Index(FacebookContext context)
        {
            if (ModelState.IsValid)
            {
                var user = await context.Client.GetCurrentUserAsync<MyAppUser>();

                Session.Remove("user");
                Session.Add("user",user);

                bool isAdmin = false;
                
                Facebook.FacebookClient client = new Facebook.FacebookClient(context.AccessToken);

                JObject o = new JObject();
                //Get News Feed
                try
                {
                    dynamic request = client.Get("fql",
                        new
                        {
                            q = new
                            {
                                query1 = "SELECT role FROM app_role WHERE developer_id ='" + context.UserId + "' AND application_id = '" + ConfigurationManager.AppSettings["Facebook:AppId"] + "'",
                            }
                        });
                    o = JObject.Parse(Convert.ToString(request));

                    String CountValue = o.SelectToken("data[0].fql_result_set[0].role").ToString();
                    if (CountValue == "administrators")
                    {
                        isAdmin = true;
                    }
                    

                }
                catch
                {
                    isAdmin = false;
                   
                }

                SaveLoginAttempt(context.UserId,user.Name,Request.UserHostAddress.ToString(),isAdmin,Request.UserAgent.ToString());
                

                if (!isAdmin)
                {
                    return View("NoAccess");
                }


              
                return View(user);
            }

            return View("Error");
        }

        public ActionResult Push(string FacebookId, string op) 
        {
            ViewBag.result = "";

            Int64 test = 0;

            Int64.TryParse(FacebookId, out test);

            if (string.IsNullOrWhiteSpace(FacebookId) || string.IsNullOrWhiteSpace(op) || test < 1 )
            {
                ViewBag.result = "Parametrelerde hata var!";
                return View("Index", Session["user"]);
            }

            if ((Session["user"] as MyAppUser).Id == FacebookId)
            {
                ViewBag.result = "Yetkili bir abimize ayak yapamazsın...";
                return View("Index", Session["user"]);
            }

            switch (op)
            {
                case "":
                default:
                    break;
                case "1": //Yasakla

                    if (isBanned(FacebookId) == true)
                    {
                        ViewBag.result = "Bu kişiye zaten yol vermişiz...";
                    }
                    else 
                    {
                        try
                        {
                            Ban(FacebookId);
                            ViewBag.result = "Bu kişiye yol verilmiştir.";
                        }
                        catch (Exception Ex)
                        {
                            ViewBag.result = "Yollarda sıkıntı var neden ; " + Ex.Message;
                        }
                    }

                    break;
                case "0": //Çıkar

                    if (isBanned(FacebookId) == false)
                    {
                        ViewBag.result = "Bu kişi zaten azledilmiş veya yokmuş...";
                    }
                    else 
                    {
                        try
                        {
                            Forgive(FacebookId);
                            ViewBag.result = "Bu kişi azledilmiştir.";
                        }
                        catch (Exception exm)
                        {
                            ViewBag.result = "Bu kişi azledilirken bir sıkıntı oldu neden ; " + exm.Message;                            
                        }
                    }

                    break;
            }

            return View("Index",Session["user"]);
        }

        // This action will handle the redirects from FacebookAuthorizeFilter when 
        // the app doesn't have all the required permissions specified in the FacebookAuthorizeAttribute.
        // The path to this action is defined under appSettings (in Web.config) with the key 'Facebook:AuthorizationRedirectPath'.
        public ActionResult Permissions(FacebookRedirectContext context)
        {
            if (ModelState.IsValid)
            {
                return View(context);
            }

            return View("Error");
        }
    }
}
