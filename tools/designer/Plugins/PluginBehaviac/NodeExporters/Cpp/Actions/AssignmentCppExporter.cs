/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Tencent is pleased to support the open source community by making behaviac available.
//
// Copyright (C) 2015 THL A29 Limited, a Tencent company. All rights reserved.
//
// Licensed under the BSD 3-Clause License (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at http://opensource.org/licenses/BSD-3-Clause
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Behaviac.Design;
using Behaviac.Design.Nodes;
using PluginBehaviac.Nodes;
using PluginBehaviac.DataExporters;

namespace PluginBehaviac.NodeExporters
{
    public class AssignmentCppExporter : NodeCppExporter
    {
        protected override bool ShouldGenerateClass(Node node)
        {
            return true;
        }

        protected override void GenerateConstructor(Node node, StreamWriter stream, string indent, string className)
        {
            base.GenerateConstructor(node, stream, indent, className);

            Assignment assignment = node as Assignment;
            Debug.Check(assignment != null);

            if (assignment.Opr != null)
            {
                RightValueCppExporter.GenerateClassConstructor(assignment.Opr, stream, indent, "opr");
            }
        }

        protected override void GenerateMember(Node node, StreamWriter stream, string indent)
        {
            base.GenerateMember(node, stream, indent);

            Assignment assignment = node as Assignment;
            Debug.Check(assignment != null);

            if (assignment.Opr != null)
            {
                RightValueCppExporter.GenerateClassMember(assignment.Opr, stream, indent, "opr");
            }
        }

        protected override void GenerateMethod(Node node, StreamWriter stream, string indent)
        {
            base.GenerateMethod(node, stream, indent);

            Assignment assignment = node as Assignment;
            Debug.Check(assignment != null);

            stream.WriteLine("{0}\t\tvirtual EBTStatus update_impl(Agent* pAgent, EBTStatus childStatus)", indent);
            stream.WriteLine("{0}\t\t{{", indent);
            stream.WriteLine("{0}\t\t\tBEHAVIAC_UNUSED_VAR(pAgent);", indent);
            stream.WriteLine("{0}\t\t\tBEHAVIAC_UNUSED_VAR(childStatus);", indent);
            stream.WriteLine("{0}\t\t\tEBTStatus result = BT_SUCCESS;", indent);

            if (assignment.Opl != null && assignment.Opr != null)
            {
                if (assignment.Opl.IsPar)
                {
                    ParInfo par = assignment.Opl.Value as ParInfo;
                    if (par != null)
                    {
                        RightValueCppExporter.GenerateCode(assignment.Opr, stream, indent + "\t\t\t", assignment.Opr.NativeType, "opr", "opr");
                        uint id = Behaviac.Design.CRC32.CalcCRC(par.Name);
                        stream.WriteLine("{0}\t\t\tBEHAVIAC_ASSERT(behaviac::MakeVariableId(\"{1}\") == {2}u);", indent, par.Name, id);
                        stream.WriteLine("{0}\t\t\tpAgent->SetVariable(\"{1}\", opr, {2}u);", indent, par.Name, id);
                    }
                }
                else
                {
                    string opl = VariableCppExporter.GenerateCode(assignment.Opl, stream, indent + "\t\t\t", string.Empty, string.Empty, "opl");
                    RightValueCppExporter.GenerateCode(assignment.Opr, stream, indent + "\t\t\t", assignment.Opr.NativeType, "opr", "opr");
                    stream.WriteLine("{0}\t\t\t{1} = opr;", indent, opl);

                    VariableCppExporter.PostGenerateCode(assignment.Opl, stream, indent + "\t\t\t", assignment.Opl.NativeType, "opl", string.Empty);
                }

                if (assignment.Opr.IsMethod)
                {
                    RightValueCppExporter.PostGenerateCode(assignment.Opr, stream, indent + "\t\t\t", assignment.Opr.NativeType, "opr", string.Empty);
                }
            }

            stream.WriteLine("{0}\t\t\treturn result;", indent);
            stream.WriteLine("{0}\t\t}}", indent);
        }
    }
}
