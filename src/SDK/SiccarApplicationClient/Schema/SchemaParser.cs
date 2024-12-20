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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Json.Schema;
using Json.Pointer;

namespace Siccar.Application.Client.Schema
{
    /// <summary>
    /// Utilities to extract the meaning of fields within a JSONSchema
    /// </summary>
    public class SchemaParser
    {
        private readonly JsonSchema _jsonSchema;

        private JsonSerializerOptions opts = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public SchemaParser(JsonSchema schema)
        {
            // is ToString or Via a Stream faster
            _jsonSchema = schema;
        }

        public string GetField(string fieldName)
        {
            IJsonSchemaKeyword value = _jsonSchema.Keywords.FirstOrDefault(k => k.Keyword() == fieldName);

            switch (value)
            {
                case Json.Schema.TypeKeyword:
                    return ((Json.Schema.TypeKeyword)value).Type.ToString();
                case Json.Schema.IdKeyword:
                    return ((Json.Schema.IdKeyword)value).Id.ToString(); // its actually a URI
                case Json.Schema.TitleKeyword:
                    return ((Json.Schema.TitleKeyword)value).Value;
                case Json.Schema.DescriptionKeyword:
                    return ((Json.Schema.DescriptionKeyword)value).Value ?? "";
                case Json.Schema.DefaultKeyword:
                    return ((Json.Schema.DefaultKeyword)value).Value.ToString(); // not ok its a Json Element
                case Json.Schema.EnumKeyword:
                    return ((Json.Schema.EnumKeyword)value).Values.ToString();
                default:
                    return "";
            }
        }

        public List<string> GetEnum()
        {
            EnumKeyword value = (EnumKeyword)_jsonSchema.Keywords.FirstOrDefault(k => k.Keyword() == "enum");

            if (value == null) return null;

            var enums = new List<string>();
            foreach (var item in value.Values)
            {
                enums.Add(item.ToString());
            }

            return enums;
        }

        public Dictionary<string, JsonSchema> GetProperties()
        {
            var tempItems = new Dictionary<string, JsonSchema>();
            PropertiesKeyword value = (PropertiesKeyword)_jsonSchema.Keywords.Single(k => k.Keyword() == "properties");

            foreach (var t in value.Properties)
            {
                tempItems.Add(t.Key, t.Value);
            }
            return (Dictionary<string, JsonSchema>)value.Properties;
        }

        /// <summary>
        /// Retrieve an Named property from a Schema
        /// </summary>
        /// <param name="attrname"></param>
        /// <returns></returns>
        //public JsonElement GetProperty(string attrname)
        //{
        //    return _jsonSchema.Keywords[attrname];

        //}
    }
}