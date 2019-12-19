using System;
using Microsoft.AspNetCore.Identity;

namespace frep.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime DateAdd { get; set; }
        public string Path { get; set; }
        public bool IsSecured { get; set; }
    }
}