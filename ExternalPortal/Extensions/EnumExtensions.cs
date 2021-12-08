using ExternalPortal.Enums;
using ExternalPortal.ViewModels.Tasks;
using GovUkDesignSystem.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ofgem.API.GGSS.Domain.Enums;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Get human-readable version of enum.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns>effective DisplayAttribute.Name of given enum</returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>();

            var displayName = displayAttribute?.GetName();
            return displayName ?? enumValue.ToString();
        }

        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        public static TaskPropertyName NextValue(this TaskValueViewModel vm)
        {
            if (vm.Status == TaskValueStatus.Completed) return vm.PropertyName.Next();
            return vm.PropertyName;
        }

        /// <summary>
        /// Get CssClass of TaskStatus
        /// </summary>
        /// <returns>Return the CssClass depending on given TaskStatus</returns>
        public static string GetDisplayTag(this TaskStatus taskState)
        {
            return taskState switch
            {
                TaskStatus.CannotStartYet => "govuk-tag--grey",
                TaskStatus.NotStarted => "govuk-tag--grey",
                TaskStatus.InProgress => "govuk-tag--blue",
                TaskStatus.Approved => "govuk-tag--green",
                _ => "",
            };
        }

        public static string GetDisplayTag(this string applicationStatus)
        {
            return applicationStatus switch
            {
                "Stage One Submitted" => "govuk-tag--blue",
                "Draft" => "govuk-tag--yellow",
                "Rejected" => "govuk-tag--red",
                "Stage One Approved" => "govuk-tag--green",
                _ => "govuk-tag--blue",
            };
        }

        /// <summary>
        /// Get human-readable version of enum.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns>effective GovUkRadioCheckboxLabelTextAttribute.Text of given enum</returns>
        public static string GetLabelText(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<GovUkRadioCheckboxLabelTextAttribute>();

            var displayName = displayAttribute?.Text;
            return displayName ?? enumValue.ToString();
        }

        public static int GetStatusInt(this Enum applicationStatus)
        {
            return applicationStatus switch
            {
                ApplicationStatus.StageOneSubmitted => 1,
                ApplicationStatus.StageOneApproved => 2,
                ApplicationStatus.StageTwoSubmitted => 3,
                ApplicationStatus.StageTwoApproved => 4,
                ApplicationStatus.StageThreeSubmitted => 5,
                ApplicationStatus.StageThreeApproved => 6,
                _ => 0
            };
        }
    }
}
