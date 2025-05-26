using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Models;

public class TestModel
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Message {get; set;}
}
