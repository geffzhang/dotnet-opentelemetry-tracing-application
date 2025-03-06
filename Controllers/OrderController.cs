using Microsoft.AspNetCore.Mvc;
using TracingDemo.Models;
using TracingDemo.Telemetry;
using System.Diagnostics;

namespace TracingDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private static readonly List<Order> _orders = new();
    private readonly HttpClient _httpClient;

    public OrderController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Order order)
    {
        using var activity = TracingInstrumentation.ActivitySource.StartActivity("CreateOrder");
        activity?.SetTag("order.customer", order.CustomerName);
        activity?.SetTag("order.amount", order.Amount);

        // Simulate external API call
        using var validateActivity = TracingInstrumentation.ActivitySource.StartActivity("ValidateCustomer");
        await _httpClient.GetAsync("https://httpstat.us/200?sleep=100");
        
        order.Id = _orders.Count + 1;
        _orders.Add(order);

        activity?.SetTag("order.id", order.Id);
        return Ok(order);
    }

    [HttpGet("{id}")]
    public IActionResult GetOrder(int id)
    {
        using var activity = TracingInstrumentation.ActivitySource.StartActivity("GetOrder");
        activity?.SetTag("order.id", id);

        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Order not found");
            return NotFound();
        }

        return Ok(order);
    }
}
