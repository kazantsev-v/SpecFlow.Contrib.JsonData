﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SpecFlow.Contrib.JsonData.SpecFlowPlugin.DataSources.Selectors;

namespace SpecFlow.Contrib.JsonData.SpecFlowPlugin.DataSources
{
    public class ExternalDataSpecification
    {
        public DataSource DataSource { get; }
        public IDictionary<string, DataSourceSelector> SpecifiedFieldSelectors { get; }

        public ExternalDataSpecification(DataSource dataSource, IDictionary<string, DataSourceSelector> specifiedFieldSelectors = null)
        {
            SpecifiedFieldSelectors = specifiedFieldSelectors == null ? null :
                new ReadOnlyDictionary<string, DataSourceSelector>(
                    new Dictionary<string, DataSourceSelector>(specifiedFieldSelectors, FieldNameComparer.Value));
            DataSource = dataSource;
        }

        public DataTable GetExampleRecords(string[] examplesHeaderNames)
        {
            //TODO: handle hierarchical data
            if (!DataSource.IsDataTable)
                throw new ExternalDataPluginException("Hierarchical data sources are not supported yet.");

            var dataTable = DataSource.AsDataTable;

            var headerNames = examplesHeaderNames ?? GetTargetHeader(dataTable.Header);
            var fieldSelectors = GetFieldSelectors(headerNames);

            var result = new DataTable(headerNames);
            foreach (var dataRecord in dataTable.Items)
            {
                result.Items.Add(new DataRecord(
                    fieldSelectors.Select(targetFieldSelector =>
                        new KeyValuePair<string, DataValue>(
                            targetFieldSelector.Key,
                            targetFieldSelector.Value.Evaluate(new DataValue(dataRecord))))));
            }
            return result;
        }

        private Dictionary<string, DataSourceSelector> GetFieldSelectors(string[] headerNames)
        {
            return headerNames.ToDictionary(h => h, GetSourceFieldSelector);
        }

        private DataSourceSelector GetSourceFieldSelector(string targetField)
        {
            if (SpecifiedFieldSelectors != null &&
                SpecifiedFieldSelectors.TryGetValue(targetField, out var selector))
                return selector;
            return new FieldNameSelector(targetField);
        }

        private string[] GetTargetHeader(string[] dataTableHeader)
        {
            if (SpecifiedFieldSelectors == null) return dataTableHeader;
            return SpecifiedFieldSelectors.Keys.Concat(dataTableHeader.Where(sourceField => !SpecifiedFieldSelectors.Values.Any(selector => FieldUsedBy(sourceField, selector)))).ToArray();
        }

        private bool FieldUsedBy(string field, DataSourceSelector selector)
            => selector is FieldNameSelector fieldNameSelector && fieldNameSelector.Name.Equals(field);
    }
}
