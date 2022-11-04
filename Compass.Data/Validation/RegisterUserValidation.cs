using Compass.Data.Data.ViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compass.Data.Validation
{
    public class RegisterUserValidation : AbstractValidator<RegisterUserVM>
    {
        public RegisterUserValidation()
        {
            RuleFor(r => r.Email).EmailAddress().NotEmpty();
            RuleFor(r => r.Password).NotEmpty().MinimumLength(6);
            RuleFor(r => r.ConfirmPassword).NotEmpty().MinimumLength(6);
        }
    }
}
