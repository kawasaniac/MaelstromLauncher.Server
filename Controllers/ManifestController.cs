using Microsoft.AspNetCore.Mvc;

namespace MaelstromLauncher.Server.Controllers
{
    public class ManifestController : Controller
    {
        // GET: ManifestController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ManifestController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ManifestController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ManifestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ManifestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ManifestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ManifestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ManifestController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
