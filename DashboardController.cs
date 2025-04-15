using IoT.Services;
using Microsoft.AspNetCore.Mvc;

namespace IoT.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ArduinoCloudService _arduinoService;

        public DashboardController(ArduinoCloudService arduinoService)
        {
            _arduinoService = arduinoService;
        }

        public async Task<IActionResult> Index()
        {
            var allProps = await _arduinoService.GetAllThingPropertiesAsync();

            var filteredProps = allProps.Where(p =>
                p.name == "temperature" ||
                p.name == "emergency_button" ||
                p.name == "location"
            ).ToList();

            return View(filteredProps);
        }
        [HttpPost]
        public async Task<IActionResult> ToggleEmergency()
        {
            // Optional: Get current state
            var props = await _arduinoService.GetAllThingPropertiesAsync();
            var emergency = props.FirstOrDefault(p => p.name == "emergency_button");

            bool newValue = emergency?.last_value?.ToString().ToLower() != "true";

            await _arduinoService.UpdateThingPropertyAsync("emergency_button", newValue);

            return RedirectToAction("Index");
        }
        public IActionResult Emergency()
        {
            return View();
        }
    }
}
