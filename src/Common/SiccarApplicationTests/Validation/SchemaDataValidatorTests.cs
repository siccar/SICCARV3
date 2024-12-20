﻿/*
* Copyright (c) 2024 Siccar (Registered co. Wallet.Services (Scotland) Ltd).
* All rights reserved.
*
* This file is part of a proprietary software product developed by Siccar.
*
* This source code is licensed under the Siccar Proprietary Limited Use License.
* Use, modification, and distribution of this software is subject to the terms
* and conditions of the license agreement. The full text of the license can be
* found in the LICENSE file or at https://github.com/siccar/SICCARV3/blob/main/LICENCE.txt.
*
* Unauthorized use, copying, modification, merger, publication, distribution,
* sublicensing, and/or sale of this software or any part thereof is strictly
* prohibited except as explicitly allowed by the license agreement.
*/

using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Siccar.Application.Validation;

namespace SiccarApplicationTests.Validation
{
    public class SchemaDataValidatorTests
    {
        private readonly SchemaDataValidator _underTest;
        private const string schemaString = "{ \"$schema\": \"http://json-schema.org/draft-07/schema\", \"$id\": \"https://siccar.net/schema/SimpleSend.json\", \"type\": \"object\", \"title\": \"Simple Data\", \"description\": \"A Simple Data item between participants\", \"properties\": { \"name\": { \"$id\": \"name\", \"type\": \"string\", \"title\": \"First Name\", \"description\": \"The applicants first name.\" }, \"surname\": { \"$id\": \"surname\", \"type\": \"string\", \"title\": \"Surname\", \"description\": \"The applicants surname\" }, \"actioncondition\": { \"$id\": \"actioncondition\", \"title\": \"Action Condition\", \"description\": \"Source of data for the action condition\", \"type\": \"boolean\" } } }";
        public SchemaDataValidatorTests()
        {
            _underTest = new SchemaDataValidator();
        }
        public class ValidateSchemaData : SchemaDataValidatorTests
        {
            [Fact]
            public void ShouldReturnTrue_WhenDataIsValid()
            {
                var data = "{ \"name\": \"Joe\", \"surname\": \"Bloggs\", \"actioncondition\": false }";

                var result = _underTest.ValidateSchemaData(schemaString, data);

                Assert.True(result.isValid);
            }

            [Theory]
            [InlineData("{ \"name\": 10, \"surname\": 10.0, \"actioncondition\": true }")]
            [InlineData("{ \"name\": \"Joe\", \"surname\": \"Bloggs\", \"actioncondition\": \"not a bool\" }")]
            [InlineData("{ \"name\": false, \"surname\": \"Bloggs\", \"actioncondition\": \"not a bool\" }")]
            [InlineData("{ \"name\": \"Joe\", \"surname\": 10.0, \"actioncondition\": \"not a bool\" }")]
            public void ShouldReturnFalse_WhenDataIsInvalid(string data)
            {
                var result = _underTest.ValidateSchemaData(schemaString, data);

                Assert.False(result.isValid);
            }

            [Fact]
            public void ShouldReturn_OneValidationError()
            {
                var data = "{ \"name\": 10, \"surname\": \"Bloggs\", \"actioncondition\": false }";

                var result = _underTest.ValidateSchemaData(schemaString, data);

                Assert.Contains("name", result.validationMessage);
                Assert.DoesNotContain("surname", result.validationMessage);
                Assert.DoesNotContain("actioncondition", result.validationMessage);
                // Outputs the location of the validation source.
                Assert.Contains("#/properties/name/type", result.validationMessage);
            }

            [Fact]
            public void ShouldReturn_MultipleValidationError()
            {
                var data = "{ \"name\": 10, \"surname\": \"Bloggs\", \"actioncondition\": \"test\" }";

                var result = _underTest.ValidateSchemaData(schemaString, data);

                Assert.Contains("name", result.validationMessage);
                Assert.Contains("actioncondition", result.validationMessage);
                Assert.DoesNotContain("surname", result.validationMessage);
                // Outputs the location of the validation source.
                Assert.Contains("#/properties/name/type", result.validationMessage);
                Assert.Contains("#/properties/actioncondition/type", result.validationMessage);
            }
        }
    }
}