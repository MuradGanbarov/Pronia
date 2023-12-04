using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;

namespace Pronia.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;

        public LayoutService(AppDbContext context,IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }
        public async Task<Dictionary<string,string>> GetSettings()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s=>s.Key,s=>s.Value);
            return settings;
        }

        
        
    }
}
