using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class ReactionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public Guid SenderId { get; set;}
}
