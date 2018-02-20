using FluentValidation;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Validators.Interfaces;
using System;
using System.Linq;

namespace Lifvs.Common.Validators
{
    public class LoginUserCredentialValidator : IValidatorService<AudienceCredentials>
    {
        private readonly IExceptionManager _exception;
        public LoginUserCredentialValidator(IExceptionManager exception)
        {
            _exception = exception;
        }
        public void Validate(AudienceCredentials input)
        {
            if (input == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");
            var validator = new LoginUserCredentialValidator2();
            var validatorResult = validator.Validate(input);
            if (!validatorResult.IsValid)
            {
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", validatorResult.Errors.Select(x => x.ErrorMessage).ToList());
            }
        }
    }
    class LoginUserCredentialValidator2 : AbstractValidator<AudienceCredentials>
    {
        private static readonly string[] DeviceTypes = { "ANDROID", "IOS" };
        public LoginUserCredentialValidator2()
        {
           
            RuleFor(x => x.Username).NotEmpty().WithMessage("Användarnamn kan inte vara tomt.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Lösenord kan inte vara tomt.");
            RuleFor(x => x.DeviceId).NotEmpty().WithMessage("Enhetsid kan inte vara tomt.");
            RuleFor(x => x.DeviceType).NotEmpty().WithMessage("Enhetstyp kan inte var tomt.").Must(ValidateDeviceType).WithMessage("Ogiltig enhetstyp.");
        }
        private static bool ValidateDeviceType(string deviceType)
        {
            return string.IsNullOrEmpty(deviceType) || DeviceTypes.Any(item => string.Equals(item, deviceType, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
