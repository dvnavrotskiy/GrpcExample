using Microsoft.AspNetCore.Mvc;
using SecondService.Services;

namespace SecondService.Controllers;

[ApiController, Route("/")]
public class HomeController(ILogger<HomeController> logger, DataService dataService) : ControllerBase
{
    [HttpGet]
    public ActionResult Get()
    {
        logger.LogInformation("SecondService Home Controller executed.");
        var status = dataService.GetStatus("http default");
        return Ok(status);
    }
}