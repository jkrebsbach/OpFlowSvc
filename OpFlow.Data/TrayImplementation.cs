using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpFlow.Data
{
    public class TrayImplementationStep
    {
        public string Specialty { get; set; }
        public List<string> Activities { get; set; }
        public List<string> Outputs { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Timing { get; set; }
        public List<string> Tools { get; set; }
    }
    public class ImplementationAttachment
    {
        public int AttachmentID { get; set; }
        public int Target { get; set; }
        public string Filename { get; set; }
    }
    public class ProposedTrayOrgChart
    {
        public int OrgChartID { get; set; }
        public string Filename { get; set; }
    }
    public class TrayImplementation
    {
        public string Overview { get; set; }
        public string Title { get; set; }
        public List<TrayImplementationStep> Steps { get; set; }
        public List<ImplementationAttachment> Attachments { get; set; }

        public TrayImplementation()
        {
            Attachments = new List<ImplementationAttachment>();
        }

        public static List<TrayImplementation> GetImplementationSteps(IEnumerable<ImplementationAttachment> attachments)
        {
            var result = new List<TrayImplementation>()
            {
                GetPlanningImplementationSteps(),
                GetCollectImplementationSteps(),
                GetCreateImplementationSteps(),
                GetConductImplementationSteps(),
                GetRollOutImplementationSteps(),
                GetSustainmentImplementationSteps()
            };

            foreach (var attachment in attachments ?? new List<ImplementationAttachment>())
            {
                result[attachment.Target].Attachments.Add(attachment);
            }

            return result;
        }

        public static TrayImplementation GetPlanningImplementationSteps()
        {
            var result = new TrayImplementation()
            { 
                Title = "Planning / Set-Up",
            Overview = "Define the scope of tray types for rationalization. Setting up the OpFlow system and importing required data.",
            Steps = new List<TrayImplementationStep>()
            {
                new TrayImplementationStep()
                {
                    Specialty = "Admininistration",
                    Activities = new List<string>() {
                        "Conduct kick-off project sponsor team meeting",
                        "Schedule weekly implementation team meetings",
                        "Identify roles/responsibilities during Steering committee meeting",
                        "Create first pass project plan"
                    },
                    Outputs = new List<string>() {
                        "Communication Plan",
                        "First Pass Project Plan",
                        "Identify key team members"
                    },
                    Roles = new List<string>() {
                        "STERIS Account Manager",
                        "OpFlow project sponsor",
                        "Hospital project sponsor",
                        "Hospital surgical services leader",
                        "Hospital SPD leader",
                        "Service coordinators",
                        "Surgeon champions"
                    },
                    Timing = new List<string>() {
                        "Kick off meeting at project start up (90 mins)",
                        "Bi-weekly project sponsor meetings (60 mins)",
                        "Weekly implementation team meetings (60 mins)"
                    },
                    Tools = new List<string>() {
                        "OpFlow project plan template",
                        "OpFlow Communication Plan Template"
                    }
                },
                new TrayImplementationStep()
                {
                    Specialty = "Clinical",
                    Activities = new List<string>()
                    {
                        "Review OpFlow training materials and videos",
                        "Identify target trays",
                        "Define roles and responsibilities for clinical team",
                        "Setup communication and rollout plan"
                    },
                    Outputs = new List<string>()
                    {
                        "Identified target trays and configurations",
                        "Communication Plan"
                    },
                    Roles = new List<string>()
                    {
                        "STERIS account manager",
                        "OpFlow project sponsor",
                        "Hospital project sponsor",
                        "Hospital surgical services leader",
                        "Hospital SPD leader",
                        "Service coordinators",
                        "Surgeon champions"
                    },
                    Timing = new List<string>()
                    {
                        "Kick off meeting at project start up (90 mins)",
                        "Bi-weekly project sponsor meetings (60 mins)"
                    },
                    Tools = new List<string>() {
                        "OpFlow Communication plan template",
                        "Training materials"
                    }
                },
                new TrayImplementationStep() {
                    Specialty = "Business Processes",
                    Activities = new List<string>()
                    {
                        "Deliver data specifications to hospital and review returned sample files",
                        "Conduct data validation on returned data samples",
                        "Set up base user data in OpFlow",
                        "Setup OR and OR section / group definitions and service line names"
                    },
                    Outputs = new List<string>()
                    {
                        "Configured users, OR and section/group information and service lines in OpFlow",
                        "Validated data samples"
                    },
                    Roles = new List<string>()
                    {
                        "Hospital surgical services leader",
                        "Hospital SPD leader",
                        "Service Line lead",
                        "OpFlow rationalization lead",
                        "STERIS implementation lead"
                    },
                    Timing = new List<string>()
                    {
                        "Set up data entered into OpFlow upon receipt of returned information: one time set up (1-3 days)"
                    },
                    Tools = new List<string>()
                    {
                        "Base information/data specification template",
                        "OpFlow administration module for base data creation"
                    }
                },
                new TrayImplementationStep() {
                    Specialty = "Technology",
                    Activities = new List<string>()
                    {
                        "Set up hospital-specific instance of OpFlow",
                        "Import preference cards, item master (disposables), tray type definitions (instruments), case schedule data as specified",
                        "Conduct data cleansing on all imported data"
                    },
                    Outputs = new List<string>()
                    {
                        "Configured instance of OpFlow application with validated preference card, item master and schedule data."
                    },
                    Roles = new List<string>()
                    {
                        "OpFlow data lead",
                        "OpFlow rationalization lead",
                        "Hospital service line lead",
                        "Hospital EHR/Schedule lead",
                        "Hospital surgical team lead"
                    },
                    Timing = new List<string>()
                    {
                        "Set up data entered into OpFlow upon receipt of returned information: one time set up (1-3 days)"
                    },
                    Tools = new List<string>()
                    {
                        "OpFlow data import facility in OpFlow Admin module",
                        "OpFlow administration module for base data creation"
                    }
                }
            }
        };

        return result;
        }

        public static TrayImplementation GetCollectImplementationSteps()
        {
            var result = new TrayImplementation()
            { 
                Overview = "Conduct information gathering and collect the data required within the OR during the close phase of the surgery.",
                Title = "Collect Tray Data",
                Steps = new List<TrayImplementationStep>()
                {
                    new TrayImplementationStep()
                    {
                        Specialty = "Admininistration",
                        Activities = new List<string>()
                        {
                            "Schedule OR staff communication updates",
                            "Conduct weekly implementation meetings",
                            "Conduct bi-weekly project sponsor meetings",
                            "Conduct daily collection team huddles"
                        },
                        Outputs = new List<string>()
                        {
                            "Communication Plan",
                            "OR communication updates"
                        },
                        Roles = new List<string>()
                        {
                            "STERIS implementation lead",
                            "Surgical team leads",
                            "EHR/schedule leads",
                            "Service line leads",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Bi-weekly project sponsor meetings (60 mins)",
                            "Weekly implementation team meetings (60 mins)",
                            "Daily collection team meetings (30 mins)"
                        },
                        Tools = new List<string>()
                        {
                            "OR communication email update templates",
                            "OpFlow Communication Plan Template"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Clinical",
                        Activities = new List<string>()
                        {
                            "Schedule counts for targeted trays",
                            "Identify cases using targeted trays",
                            "Begin data collection"
                        },
                        Outputs = new List<string>()
                        {
                            "Targeted tray list",
                            "Count and data collection schedule"
                        },
                        Roles = new List<string>()
                        {
                            "STERIS implementation lead",
                            "Surgical team leads",
                            "EHR/schedule leads",
                            "Service line leads",
                            "Surgeon champions"
                        },
                        Timing = new List<string>()
                        {
                            "Daily counts for target trays and identified cases (5 mins each count)"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow Case/counts module",
                            "Communication plan for tray updates"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Business Processes",
                        Activities = new List<string>()
                        {
                            "Check case count session to identify any anomalies",
                            "Conduct counts on cases with targeted trays",
                            "Add any instruments to tray as identified by count team"
                        },
                        Outputs = new List<string>()
                        {
                            "Case/Tray count",
                            "Tray instrument list"
                        },
                        Roles = new List<string>()
                        {
                            "STERIS implementation lead",
                            "STERIS implementation specialist",
                            "Surgical team leads",
                            "EHR/schedule leads",
                            "Service line leads"
                        },
                        Timing = new List<string>()
                        {
                            "Daily counts for target trays and identified cases (5 mins each count)"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow Case/counts module"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Technology",
                        Activities = new List<string>()
                        {
                            "Validate tray configurations data integrity",
                            "Correct any data/count/anomalies"
                        },
                        Outputs = new List<string>()
                        {
                            "Case/Tray count",
                            "Tray instrument list"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow Data lead",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Daily counts for target trays and identified cases (60 mins)"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow Case/counts module"
                        }
                    }
                }
            };

            return result;
        }

        public static TrayImplementation GetCreateImplementationSteps()
        {
            var result = new TrayImplementation()
            {
                Overview = "Create proposed tray definitions and engage service line leaders to gather feedback and provide review.",
                Title = "Create Proposed Tray Configurations",
                Steps = new List<TrayImplementationStep>()
                {
                    new TrayImplementationStep()
                    {
                        Specialty = "Admininistration",
                        Activities = new List<string>()
                        {
                            "Conduct OR staff communication updates",
                            "Conduct surgeon champion updates",
                            "Conduct weekly implementation meetings",
                            "Conduct bi-weekly project sponsor meetings",
                            "Conduct service line communication with projected changes",
                            "Conduct daily collection team huddles"
                        },
                        Outputs = new List<string>()
                        {
                            "Communication Plan",
                            "OR communication updates"
                        },
                        Roles = new List<string>()
                        {
                            "STERIS implementation lead",
                            "Surgical team leads",
                            "EHR/schedule leads",
                            "Service line leads",
                            "Surgeon",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Bi-weekly project sponsor meetings (60 mins)",
                            "Weekly implementation team meetings (60 mins)",
                            "Daily collection team meetings (30 mins)"
                        },
                        Tools = new List<string>()
                        {
                            "OR communication email update templates",
                            "OpFlow Communication Plan Template"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Clinical",
                        Activities = new List<string>()
                        {
                            "Create proposed tray definitions",
                            "Adjust trays based on surgeon feedback for instruments required",
                            "Review proposed tray with service line leads"
                        },
                        Outputs = new List<string>()
                        {
                            "Proposed tray definitions"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow rationalization lead",
                            "Surgical team leads",
                            "EHR/schedule leads",
                            "Service line leads"
                        },
                        Timing = new List<string>()
                        {
                            "Daily reviews and/or on-demand"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Business Processes",
                        Activities = new List<string>()
                        {
                            "Conduct service line review of tray counts",
                            "Edit tray definitions based on instrument usage and surgeon feedback",
                            "Perform comparable analysis to OpFlow baseline tray",
                            "Add buffer and safety instruments",
                            "Review proposed tray with service line leads",
                            "Introduce proposed tray and potential value model from ROI"
                        },
                        Outputs = new List<string>()
                        {
                            "Proposed tray configurations",
                            "Comparison of proposed tray to OpFlow baseline tray"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow rationalization lead",
                            "Surgical team leads",
                            "EHR/schedule leads",
                            "Service line leads"
                        },
                        Timing = new List<string>()
                        {
                            "Daily reviews and/or on-demand"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module",
                            "ROI tool"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Technology",
                        Activities = new List<string>()
                        {
                            "Prepare BI data for tray analytics",
                            "Construct tray analytics views/reports",
                            "Distribute analytics to teams",
                            "Populate ROI model"
                        },
                        Outputs = new List<string>()
                        {
                            "Tray configuration data",
                            "Rationalization, concordance and instrument usage reports"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow Data lead",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Daily reviews and/or on-demand"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module",
                            "ROI tool"
                        }
                    }
                }
            };

            return result;
        }

        public static TrayImplementation GetConductImplementationSteps()
        {
            var result = new TrayImplementation()
            {
                Overview = "Perform audits of proposed trays in the OR for cases using the assigned trays to confirm accuracy and safety buffer quantity.",
                Title = "Conduct Proposed Tray Audits",
                Steps = new List<TrayImplementationStep>()
                {
                    new TrayImplementationStep()
                    {
                        Specialty = "Admininistration",
                        Activities = new List<string>()
                        {
                            "Conduct OR staff communication updates",
                            "Conduct weekly implementation meetings",
                            "Conduct bi-weekly project sponsor meetings",
                            "Conduct daily collection team huddles"
                        },
                        Outputs = new List<string>()
                        {
                            "Communication Plan",
                            "OR communication updates",
                            "Weekly status reports on audit process and progress"
                        },
                        Roles = new List<string>()
                        {
                            "STERIS implementation lead",
                            "Surgical team leads",
                            "EHR/schedule leads",
                            "Service line leads",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Bi-weekly project sponsor meetings (60 mins)",
                            "Weekly implementation team meetings (60 mins)",
                            "Daily collection team meetings (30 mins)"
                        },
                        Tools = new List<string>()
                        {
                            "OR communication email update templates",
                            "OpFlow Communication Plan Template",
                            "Weekly status report template"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Clinical",
                        Activities = new List<string>()
                        {
                            "Review proposed tray with service line leads",
                            "Document all changes to trays and prepare for sign-off process",
                            "Surgeon and service line sign offs conducted",
                            "Review expected benefits results"
                        },
                        Outputs = new List<string>()
                        {
                            "Proposed tray definitions",
                            "Sign Off sheets"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow rationalization lead",
                            "Surgical team leads",
                            "Service line leads",
                            "STERIS implementation lead"
                        },
                        Timing = new List<string>()
                        {
                            "Daily reviews and/or on-demand"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module",
                            "Sign Off sheets",
                            "ROI tool"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Business Processes",
                        Activities = new List<string>()
                        {
                            "Review proposed trays with service line leads",
                            "Document all changes to trays and prepare for sign - off process",
                            "Surgeon and service line sign offs conducted",
                            "Update tray management and preference card systems with data changes"
                        },
                        Outputs = new List<string>()
                        {
                            "Proposed tray configurations",
                            "Comparison of proposed tray to OpFlow baseline tray"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow rationalization lead",
                            "STERIS implementation lead",
                            "Surgical team leads"
                        },
                        Timing = new List<string>()
                        {
                            "Daily reviews and/or on-demand"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module",
                            "Sign Off sheets"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Technology",
                        Activities = new List<string>()
                        {
                            "Provide packet of data containing old/new tray definitions",
                            "Provide supporting analytics"
                        },
                        Outputs = new List<string>()
                        {
                            "Tray configuration data",
                            "Rationalization, concordance and instrument usage reports"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow Data lead",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Daily reviews and/or on-demand"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module"
                        }
                    }
                }
            };

            return result;
        }

        public static TrayImplementation GetRollOutImplementationSteps()
        {
            var result = new TrayImplementation()
            {
                Overview = "Remove excess instruments from selected trays and placed in inventory. Return updated trays back into circulation.",
                Title = "New Tray Configuration Roll Out",
                Steps = new List<TrayImplementationStep>()
                {
                    new TrayImplementationStep()
                    {
                        Specialty = "Admininistration",
                        Activities = new List<string>()
                        {
                            "Conduct OR staff communication updates",
                            "Conduct weekly implementation meetings",
                            "Conduct bi-weekly project sponsor meetings",
                            "Conduct daily collection team huddles"
                        },
                        Outputs = new List<string>()
                        {
                            "Communication Plan",
                            "OR communication updates",
                            "Weekly status reports on audit process and progress"
                        },
                        Roles = new List<string>()
                        {
                            "STERIS implementation lead",
                            "Surgical team leads",
                            "EHR/schedule leads",
                            "Service line leads",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Bi-weekly project sponsor meetings (60 mins)",
                            "Weekly implementation team meetings (60 mins)",
                            "Daily collection team meetings (30 mins)"
                        },
                        Tools = new List<string>()
                        {
                            "OR communication email update templates",
                            "OpFlow Communication Plan Template",
                            "Weekly status report template"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Clinical",
                        Activities = new List<string>()
                        {
                            "Create a tray update plan for each tray in scope",
                            "Initiate changeover of the first set of trays that are not needed for cases",
                            "Update count sheets when first set of trays is updated"
                        },
                        Outputs = new List<string>()
                        {
                            "Tray update plan",
                            "Tray rollout plan"
                        },
                        Roles = new List<string>()
                        {
                            "SPD leader",
                            "Service line leads",
                            "Surgical team leads"
                        },
                        Timing = new List<string>()
                        {
                            "Twice each week"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module",
                            "Sign Off sheets",
                            "Tray rollout plan"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Business Processes",
                        Activities = new List<string>()
                        {
                            "Remove instruments no longer required",
                            "Collect and label removed instruments and return to inventory",
                            "Return updated trays for sterilization and back into circulation"
                        },
                        Outputs = new List<string>()
                        {
                            "Updated trays",
                            "Tray rollout plan"
                        },
                        Roles = new List<string>()
                        {                            
                            "SPD leader",
                            "Service line leads",
                            "Surgical team leads",
                            "STERIS implementation lead",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "1-2 tray type(s) per week (recommended)"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module",
                            "Sign Off sheets"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Technology",
                        Activities = new List<string>()
                        {
                            "Provide packet of data containing old/new tray definitions",
                            "Provide supporting analytics"
                        },
                        Outputs = new List<string>()
                        {
                            "Tray configuration data",
                            "Rationalization, concordance and instrument usage reports"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow Data lead",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Daily reviews and/or on-demand"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module"
                        }
                    }
                }
            };

            return result;
        }

        public static TrayImplementation GetSustainmentImplementationSteps()
        {
            var result = new TrayImplementation()
            {
                Overview = "Set up sample sizes for quarterly audits. Perform counts and validate analytics results for continuous tray rationalization.",
                Title = "Sustainment",
                Steps = new List<TrayImplementationStep>()
                {
                    new TrayImplementationStep()
                    {
                        Specialty = "Admininistration",
                        Activities = new List<string>()
                        {
                            "Create sustainment plan outline for continuous improvement"
                        },
                        Outputs = new List<string>()
                        {
                            "Sustainment Plan"
                        },
                        Roles = new List<string>()
                        {
                            "Admin leader",
                            "Surgical service leader",
                            "SPD leader",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "One off with quarterly review"
                        },
                        Tools = new List<string>()
                        {
                            "Sustainment plan template"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Clinical",
                        Activities = new List<string>()
                        {
                            "Review sustainment plan",
                            "Review quarterly audit results and adjust if required",
                            "Review benefits results and ROI"
                        },
                        Outputs = new List<string>()
                        {
                            "Sustainment plan",
                            "ROI model"
                        },
                        Roles = new List<string>()
                        {
                            "Surgical service leader",
                            "SPD leader",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Quarterly review"
                        },
                        Tools = new List<string>()
                        {
                            "Sustainment plan template and presentation",
                            "ROI tool"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Business Processes",
                        Activities = new List<string>()
                        {
                            "Create performance review and future tray data management plan",
                            "Review instrument additions and usage changes",
                            "Perform audits across service lines",
                            "Run ROI model to validate benefit projections"
                        },
                        Outputs = new List<string>()
                        {
                            "Data management plan",
                            "ROI model",
                            "Audit results"
                        },
                        Roles = new List<string>()
                        {
                            "STERIS account manager",
                            "Surgical services leader",
                            "SPD leader",
                            "OpFlow rationalization lead"
                        },
                        Timing = new List<string>()
                        {
                            "Quarterly review"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module"
                        }
                    },
                    new TrayImplementationStep()
                    {
                        Specialty = "Technology",
                        Activities = new List<string>()
                        {
                            "Create system flags for updated trays to be audited",
                            "Create flags to run comparative analytics on updated trays versus baseline trays",
                            "Set parameters for usage results and trigger warnings"
                        },
                        Outputs = new List<string>()
                        {
                            "Rationalization, concordance and instrument usage reports",
                            "Deviation warnings"
                        },
                        Roles = new List<string>()
                        {
                            "OpFlow rationalization lead",
                            "OpFlow Data lead"
                        },
                        Timing = new List<string>()
                        {
                            "On demand warnings",
                            "Quarterly reviews"
                        },
                        Tools = new List<string>()
                        {
                            "OpFlow tray rationalization module",
                            "OpFlow analytics module"
                        }
                    }
                }
            };

            return result;
        }
    }
}
