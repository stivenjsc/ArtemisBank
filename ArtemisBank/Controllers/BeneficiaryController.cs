using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Application.ViewModels.Beneficiary;
using ArtemisBank.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBank.Controllers
{
    [Authorize(Roles = nameof(UserRole.Client))]
    public class BeneficiaryController(IBeneficiaryService beneficiaryService) : Controller
    {
        private readonly IBeneficiaryService _beneficiaryService = beneficiaryService;

        private string GetClientId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            var clientId = GetClientId();
            var beneficiaries = await _beneficiaryService.GetByOwnerIdAsync(clientId);
            return View(beneficiaries);
        }

        public IActionResult Create()
        {
            return View(new SaveBeneficiaryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var clientId = GetClientId();
            var beneficiary = await _beneficiaryService.GetByIdAsync(id);

            if (beneficiary == null || beneficiary.OwnerId != clientId)
            {
                return Forbid();
            }
            await _beneficiaryService.DeleteAsync(id);
            TempData["Success"] = "Beneficiary removed successfully.";
            return RedirectToAction("Index");
        }
    }
}
