using FluentAssertions;
using Sqordia.Domain.Enums;
using Xunit;

namespace Sqordia.Domain.UnitTests.Enums;

public class OBNLTypeTests
{
    [Theory]
    [InlineData(OBNLType.CharitableOrganization)]
    [InlineData(OBNLType.ReligiousOrganization)]
    [InlineData(OBNLType.EducationalInstitution)]
    [InlineData(OBNLType.HealthcareOrganization)]
    [InlineData(OBNLType.EnvironmentalOrganization)]
    [InlineData(OBNLType.ArtsAndCulture)]
    [InlineData(OBNLType.SportsAndRecreation)]
    [InlineData(OBNLType.CommunityDevelopment)]
    [InlineData(OBNLType.SocialServices)]
    [InlineData(OBNLType.InternationalAid)]
    [InlineData(OBNLType.ResearchInstitution)]
    [InlineData(OBNLType.ProfessionalAssociation)]
    [InlineData(OBNLType.TradeUnion)]
    [InlineData(OBNLType.PoliticalOrganization)]
    [InlineData(OBNLType.Other)]
    public void OBNLType_ShouldHaveAllExpectedValues(OBNLType obnlType)
    {
        // Act & Assert
        obnlType.Should().BeDefined();
    }

    [Fact]
    public void OBNLType_ShouldHaveCorrectEnumValues()
    {
        // Act & Assert
        ((int)OBNLType.CharitableOrganization).Should().Be(1);
        ((int)OBNLType.ReligiousOrganization).Should().Be(2);
        ((int)OBNLType.EducationalInstitution).Should().Be(3);
        ((int)OBNLType.HealthcareOrganization).Should().Be(4);
        ((int)OBNLType.EnvironmentalOrganization).Should().Be(5);
        ((int)OBNLType.ArtsAndCulture).Should().Be(6);
        ((int)OBNLType.SportsAndRecreation).Should().Be(7);
        ((int)OBNLType.CommunityDevelopment).Should().Be(8);
        ((int)OBNLType.SocialServices).Should().Be(9);
        ((int)OBNLType.InternationalAid).Should().Be(10);
        ((int)OBNLType.ResearchInstitution).Should().Be(11);
        ((int)OBNLType.ProfessionalAssociation).Should().Be(12);
        ((int)OBNLType.TradeUnion).Should().Be(13);
        ((int)OBNLType.PoliticalOrganization).Should().Be(14);
        ((int)OBNLType.Other).Should().Be(15);
    }

    [Fact]
    public void OBNLType_ShouldHaveCorrectStringRepresentations()
    {
        // Act & Assert
        OBNLType.CharitableOrganization.ToString().Should().Be("CharitableOrganization");
        OBNLType.ReligiousOrganization.ToString().Should().Be("ReligiousOrganization");
        OBNLType.EducationalInstitution.ToString().Should().Be("EducationalInstitution");
        OBNLType.HealthcareOrganization.ToString().Should().Be("HealthcareOrganization");
        OBNLType.Other.ToString().Should().Be("Other");
    }

    [Fact]
    public void OBNLType_ShouldBeComparable()
    {
        // Act & Assert
        (OBNLType.CharitableOrganization < OBNLType.ReligiousOrganization).Should().BeTrue();
        (OBNLType.ReligiousOrganization > OBNLType.CharitableOrganization).Should().BeTrue();
        var sameValue = OBNLType.CharitableOrganization;
        (sameValue == OBNLType.CharitableOrganization).Should().BeTrue();
        (OBNLType.CharitableOrganization != OBNLType.ReligiousOrganization).Should().BeTrue();
    }

    [Fact]
    public void OBNLType_ShouldBeParsableFromString()
    {
        // Act & Assert
        Enum.TryParse<OBNLType>("CharitableOrganization", out var charitable).Should().BeTrue();
        charitable.Should().Be(OBNLType.CharitableOrganization);

        Enum.TryParse<OBNLType>("ReligiousOrganization", out var religious).Should().BeTrue();
        religious.Should().Be(OBNLType.ReligiousOrganization);

        Enum.TryParse<OBNLType>("EducationalInstitution", out var educational).Should().BeTrue();
        educational.Should().Be(OBNLType.EducationalInstitution);

        Enum.TryParse<OBNLType>("HealthcareOrganization", out var healthcare).Should().BeTrue();
        healthcare.Should().Be(OBNLType.HealthcareOrganization);

        Enum.TryParse<OBNLType>("Other", out var other).Should().BeTrue();
        other.Should().Be(OBNLType.Other);
    }

    [Fact]
    public void OBNLType_ShouldBeParsableFromInt()
    {
        // Act & Assert
        ((OBNLType)1).Should().Be(OBNLType.CharitableOrganization);
        ((OBNLType)2).Should().Be(OBNLType.ReligiousOrganization);
        ((OBNLType)3).Should().Be(OBNLType.EducationalInstitution);
        ((OBNLType)4).Should().Be(OBNLType.HealthcareOrganization);
        ((OBNLType)15).Should().Be(OBNLType.Other);
    }

    [Fact]
    public void OBNLType_ShouldHaveCorrectCount()
    {
        // Act
        var values = Enum.GetValues<OBNLType>();

        // Assert
        values.Should().HaveCount(15);
    }

    [Fact]
    public void OBNLType_ShouldBeEnumerable()
    {
        // Act
        var values = Enum.GetValues<OBNLType>().ToList();

        // Assert
        values.Should().Contain(OBNLType.CharitableOrganization);
        values.Should().Contain(OBNLType.ReligiousOrganization);
        values.Should().Contain(OBNLType.EducationalInstitution);
        values.Should().Contain(OBNLType.HealthcareOrganization);
        values.Should().Contain(OBNLType.Other);
    }

    [Fact]
    public void OBNLType_ShouldHaveValidDisplayNames()
    {
        // Act & Assert
        OBNLType.CharitableOrganization.ToString().Should().Contain("Charitable");
        OBNLType.ReligiousOrganization.ToString().Should().Contain("Religious");
        OBNLType.EducationalInstitution.ToString().Should().Contain("Educational");
        OBNLType.HealthcareOrganization.ToString().Should().Contain("Healthcare");
        OBNLType.Other.ToString().Should().Be("Other");
    }
}
