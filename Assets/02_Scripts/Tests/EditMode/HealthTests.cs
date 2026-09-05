using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class HealthTests
{
    private GameObject gameObject;
    private Health health;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject(nameof(HealthTests));
        health = gameObject.AddComponent<Health>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void LethalDamage_SetsDeathFlagBeforeDamageNotification()
    {
        bool wasDeadWhenDamaged = false;
        health.OnDamaged += _ => wasDeadWhenDamaged = health.IsDied;

        health.TakeDamage(health.MaxHP);

        Assert.That(wasDeadWhenDamaged, Is.True);
        Assert.That(health.IsDied, Is.True);
    }

    [Test]
    public void LethalDamage_NotifiesDamageBeforeDeath()
    {
        List<string> notifications = new();
        health.OnDamaged += _ => notifications.Add("Damaged");
        health.OnDied += () => notifications.Add("Died");

        health.TakeDamage(health.MaxHP);

        Assert.That(notifications, Is.EqualTo(new[] { "Damaged", "Died" }));
    }
}
