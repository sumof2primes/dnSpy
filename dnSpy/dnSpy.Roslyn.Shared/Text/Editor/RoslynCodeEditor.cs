﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Windows;
using dnSpy.Contracts.Text.Editor;
using dnSpy.Contracts.Text.Editor.Roslyn;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace dnSpy.Roslyn.Shared.Text.Editor {
	sealed class RoslynCodeEditor : IRoslynCodeEditor {
		public IDnSpyWpfTextView TextView => codeEditor.TextView;
		public IDnSpyWpfTextViewHost TextViewHost => codeEditor.TextViewHost;
		public ITextBuffer TextBuffer => codeEditor.TextBuffer;
		public object UIObject => codeEditor.UIObject;
		public IInputElement FocusedElement => codeEditor.FocusedElement;
		public FrameworkElement ScaleElement => codeEditor.ScaleElement;
		public object Tag { get; set; }

		readonly ICodeEditor codeEditor;

		public RoslynCodeEditor(RoslynCodeEditorOptions options, ICodeEditor codeEditor) {
			this.codeEditor = codeEditor;
		}

		public void Dispose() => codeEditor.Dispose();
	}
}
