using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class StatusTests
{
    private GameObject gameObject;
    private PlayerData playerData;
    private Status status;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject(nameof(StatusTests));
        playerData = ScriptableObject.CreateInstance<PlayerData>();
        status = gameObject.AddComponent<Status>();
        status.SetUpData(playerData);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
        Object.DestroyImmediate(playerData);
    }

    [Test]
    public void HasStamina_ReturnsTrue_WhenCurrentStaminaIsEnough()
    {
        Assert.That(status.HasStamina(10f), Is.True);
    }

    [Test]
    public void TakeStamina_DecreasesCurrentStamina()
    {
        bool result = status.TakeStamina(10f);

        Assert.That(result, Is.True);
        Assert.That(status.CurrStamina, Is.EqualTo(status.MaxStamina - 10f));
    }

    [Test]
    public void TakeStamina_AllowsZeroCostWithoutChangingStamina()
    {
        float staminaBeforeUse = status.CurrStamina;

        bool result = status.TakeStamina(0f);

        Assert.That(result, Is.True);
        Assert.That(status.CurrStamina, Is.EqualTo(staminaBeforeUse));
    }

    [Test]
    public void SetUpData_RefreshesSubscribedStaminaView()
    {
        Assert.That(status.TakeStamina(10f), Is.True);

        GameObject viewObject = new GameObject(nameof(SetUpData_RefreshesSubscribedStaminaView), typeof(RectTransform));
        StatusPresenter presenter = null;

        try
        {
            Slider slider = viewObject.AddComponent<Slider>();
            GaugeView staminaView = viewObject.AddComponent<GaugeView>();
            FieldInfo gaugeSliderField = typeof(GaugeView).GetField("gaugeSlider", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(gaugeSliderField, Is.Not.Null);
            gaugeSliderField.SetValue(staminaView, slider);

            presenter = new StatusPresenter(status, null, staminaView);
            status.SetUpData(playerData);

            Assert.That(slider.maxValue, Is.EqualTo(status.MaxStamina));
            Assert.That(slider.value, Is.EqualTo(status.CurrStamina));
        }
        finally
        {
            presenter?.Dispose();
            Object.DestroyImmediate(viewObject);
        }
    }

    [Test]
    public void SetUpData_InitializesHealthFromPlayerData()
    {
        const float expectedMaxHp = 175f;
        FieldInfo maxHpField = typeof(EntityData).GetField(
            "maxHP",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(maxHpField, Is.Not.Null);

        maxHpField.SetValue(playerData, expectedMaxHp);
        status.SetUpData(playerData);

        Assert.That(status.MaxHP, Is.EqualTo(expectedMaxHp));
        Assert.That(status.CurrHP, Is.EqualTo(expectedMaxHp));
    }
}
