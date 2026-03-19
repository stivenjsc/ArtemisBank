using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.ViewModels.Beneficiary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = "Client")]
    public class BeneficiaryController : Controller
    {
        private readonly IBeneficiaryService _beneficiaryService;

        public BeneficiaryController(IBeneficiaryService beneficiaryService)
        {
            _beneficiaryService = beneficiaryService;
        }

        private string GetClientId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            var clientId = GetClientId();
            var beneficiaries = await _beneficiaryService.GetByOwnerIdAsync(clientId);
            return View(beneficiaries);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SaveBeneficiaryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveBeneficiaryViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var clientId = GetClientId();

            var exists = await _beneficiaryService.BeneficiaryExistsForOwnerAsync(clientId, vm.AccountNumber);
            if (exists)
            {
                vm.HasError = true;
                vm.Error = "Este beneficiario ya está registrado.";
                return View(vm);
            }

            var result = await _beneficiaryService.AddAsync(clientId, vm.AccountNumber);

            if (!result)
            {
                vm.HasError = true;
                vm.Error = "No se pudo agregar el beneficiario. Verifique el número de cuenta.";
                return View(vm);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _beneficiaryService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
