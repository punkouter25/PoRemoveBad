using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoRemoveBad.Core;
using PoRemoveBad.Web;
using PoRemoveBad.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddCoreServices();
builder.Services.AddSingleton<ToastService>();

await builder.Build().RunAsync();
