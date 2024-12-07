// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SolarTracker.Configuration;

public class IpGeolocationClientSettings : ISettingsBase
{
    /// <summary>
    /// Api key to https://ipgeolocation.io/
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}

public class IpGeolocationClientSettingsValidator : AbstractValidator<IpGeolocationClientSettings>
{
    public IpGeolocationClientSettingsValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("IpGeolocation Api Key undefined")
            .Must(x => x != "secret").WithMessage("IpGeolocation secret Api Key Missing");
    }
}
