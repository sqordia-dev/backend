namespace Sqordia.Domain.Enums;

/// <summary>
/// Subscription plan types — ordered by tier level
/// </summary>
public enum SubscriptionPlanType
{
    /// <summary>
    /// Decouverte — Free plan for exploring the platform
    /// </summary>
    Free = 0,

    /// <summary>
    /// Essentiel — Starter plan for solo entrepreneurs ($29/mo)
    /// </summary>
    Starter = 1,

    /// <summary>
    /// Professionnel — Full-featured plan for growing businesses ($59/mo)
    /// </summary>
    Professional = 2,

    /// <summary>
    /// Entreprise — Unlimited plan for agencies and large organizations ($149/mo)
    /// </summary>
    Enterprise = 3
}

