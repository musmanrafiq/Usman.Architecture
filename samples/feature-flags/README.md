# ADR-0035: Feature Flags Demo

This sample demonstrates the **feature flag** pattern for progressive delivery.

## What this sample shows

### 1. Flag registry model

```csharp
var flag = new FeatureFlag
{
    Key = "new_checkout_flow",
    Name = "New Checkout Flow",
    Enabled = true,
    RolloutPercentage = 30,   // 30% of users
    TargetTiers = "admin,internal"  // Only these roles see the flag
};
```

### 2. Flag evaluation rules

```
