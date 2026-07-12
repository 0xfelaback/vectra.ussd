using FluentValidation;
using System.Linq;

public class EntryRequestDtoValidation : AbstractValidator<EntryRequestDto>
{
    private static readonly string[] validNetworks = { "MTN", "Airtel", "Globacom", "9mobile" };
    public EntryRequestDtoValidation()
    {

        RuleFor(x => x.msisdn)
        .NotEmpty().WithMessage("Phone number is required")
        .MinimumLength(10).WithMessage("Phone number must be at least 10 digits")
        .MaximumLength(15).WithMessage("Phone number cannot exceed 15 digits")
        .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.");

        RuleFor(x => x.network).NotEmpty().WithMessage("Network is required")
        .Must(value => validNetworks.Contains(value.ToString())).WithMessage("Please select a valid network provider.");

        RuleFor(x => x.sessionid).NotEmpty().WithMessage("Session Id is required")
        .MinimumLength(10).WithMessage("Session ID is too short")
        .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("Session ID contains invalid characters.");

        RuleFor(x => x.msg).NotEmpty().WithMessage("Mesage is required")
        .MaximumLength(10).WithMessage("Message cannot exceed 10 characters.")
        .Must(m => m.Trim().Length > 0).WithMessage("Message cannot be only whitespace.");

        RuleFor(x => x.type)
        .NotEmpty().WithMessage("Type is required")
        .IsInEnum().WithMessage("Invalid message type provided.");
    }
}