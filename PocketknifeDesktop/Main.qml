import QtQuick
import QtQuick.Controls
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import Application

ApplicationWindow {
    id: win
    visible: true
    title: "Pocketknife"
    width: 900; height: 650
    color: theme.bg

    // ---- Win95-flavored dark palette --------------------------------------
    QtObject {
        id: theme
        readonly property string bg:        "#3a3a3a"  // classic 3D face (dark variant)
        readonly property string panel:    "#2e2e2e"  // sunken panel
        readonly property string editorBg: "#1e1e1e"  // code area
        readonly property string text:     "#e6e6e6"
        readonly property string textDim:  "#9a9a9a"
        readonly property string accent:   "#0a3d91"  // Win95 titlebar blue
        // Bevel edges
        readonly property string hi1:      "#6e6e6e"  // outer light (top/left)
        readonly property string hi2:      "#4a4a4a"  // inner light
        readonly property string sh1:      "#1a1a1a"  // outer dark (bottom/right)
        readonly property string sh2:      "#2a2a2a"  // inner dark
        readonly property string fontFamily: "MS Sans Serif, Tahoma, Microsoft Sans Serif, sans-serif"
        readonly property int    fontPx:   12
    }

    font.family: theme.fontFamily
    font.pixelSize: theme.fontPx

    // ---- Reusable Win95 3D bevel ------------------------------------------
    // kind: "raised" (default), "sunken", "flat"
    component Bevel : Rectangle {
        property string kind: "raised"
        property bool pressed: false
        color: theme.bg
        border.width: 0
        radius: 0

        // outer edges
        Rectangle { // top
            anchors.left: parent.left; anchors.right: parent.right; anchors.top: parent.top
            height: 1
            color: (parent.kind === "flat") ? "transparent"
                 : ((parent.kind === "sunken" || parent.pressed) ? theme.sh1 : theme.hi1)
        }
        Rectangle { // left
            anchors.left: parent.left; anchors.top: parent.top; anchors.bottom: parent.bottom
            width: 1
            color: (parent.kind === "flat") ? "transparent"
                 : ((parent.kind === "sunken" || parent.pressed) ? theme.sh1 : theme.hi1)
        }
        Rectangle { // bottom
            anchors.left: parent.left; anchors.right: parent.right; anchors.bottom: parent.bottom
            height: 1
            color: (parent.kind === "flat") ? "transparent"
                 : ((parent.kind === "sunken" || parent.pressed) ? theme.hi1 : theme.sh1)
        }
        Rectangle { // right
            anchors.right: parent.right; anchors.top: parent.top; anchors.bottom: parent.bottom
            width: 1
            color: (parent.kind === "flat") ? "transparent"
                 : ((parent.kind === "sunken" || parent.pressed) ? theme.hi1 : theme.sh1)
        }
        // inner edges (the classic double bevel)
        Rectangle {
            anchors.left: parent.left; anchors.right: parent.right; anchors.top: parent.top
            anchors.leftMargin: 1; anchors.rightMargin: 1; anchors.topMargin: 1
            height: 1
            color: (parent.kind === "flat") ? "transparent"
                 : ((parent.kind === "sunken" || parent.pressed) ? theme.sh2 : theme.hi2)
        }
        Rectangle {
            anchors.left: parent.left; anchors.top: parent.top; anchors.bottom: parent.bottom
            anchors.leftMargin: 1; anchors.topMargin: 1; anchors.bottomMargin: 1
            width: 1
            color: (parent.kind === "flat") ? "transparent"
                 : ((parent.kind === "sunken" || parent.pressed) ? theme.sh2 : theme.hi2)
        }
        Rectangle {
            anchors.left: parent.left; anchors.right: parent.right; anchors.bottom: parent.bottom
            anchors.leftMargin: 1; anchors.rightMargin: 1; anchors.bottomMargin: 1
            height: 1
            color: (parent.kind === "flat") ? "transparent"
                 : ((parent.kind === "sunken" || parent.pressed) ? theme.hi2 : theme.sh2)
        }
        Rectangle {
            anchors.right: parent.right; anchors.top: parent.top; anchors.bottom: parent.bottom
            anchors.topMargin: 1; anchors.bottomMargin: 1; anchors.rightMargin: 1
            width: 1
            color: (parent.kind === "flat") ? "transparent"
                 : ((parent.kind === "sunken" || parent.pressed) ? theme.hi2 : theme.sh2)
        }
    }

    menuBar: MenuBar {
        Menu {
            title: "&File"
            Action { text: "&New"; onTriggered: { editor.text = ""; Editor.text = ""; Evaluator.reset() } }
            Action { text: "&Open..." }
            Action { text: "&Save" }
            MenuSeparator {}
            Action { text: "E&xit"; onTriggered: Qt.quit() }
        }
        Menu {
            title: "&Edit"
            Action { text: "&Undo"; onTriggered: editor.undo() }
            Action { text: "&Redo"; onTriggered: editor.redo() }
            MenuSeparator {}
            Action { text: "Cu&t"; onTriggered: editor.cut() }
            Action { text: "&Copy"; onTriggered: editor.copy() }
            Action { text: "&Paste"; onTriggered: editor.paste() }
        }
        Menu {
            title: "&Help"
            Action { text: "&Documentation" }
            Action { text: "&About" }
        }
    }

    ColumnLayout {
        anchors.fill: parent
        spacing: 0

        // Toolbar (Win95 raised band with beveled buttons)
        Bevel {
            kind: "raised"
            Layout.fillWidth: true
            Layout.preferredHeight: 32

            RowLayout {
                anchors.fill: parent
                anchors.margins: 4
                spacing: 4

                // tiny inline button component reused for all toolbar buttons
                component TbButton : Bevel {
                    id: btnRoot
                    property alias text: lbl.text
                    signal clicked()
                    kind: "raised"
                    Layout.preferredHeight: 22
                    Layout.preferredWidth: Math.max(56, lbl.implicitWidth + 18)
                    pressed: ma.pressed
                    Label {
                        id: lbl
                        anchors.centerIn: parent
                        anchors.horizontalCenterOffset: btnRoot.pressed ? 1 : 0
                        anchors.verticalCenterOffset:   btnRoot.pressed ? 1 : 0
                        color: theme.text
                        font.family: theme.fontFamily
                        font.pixelSize: theme.fontPx
                    }
                    MouseArea { id: ma; anchors.fill: parent; onClicked: btnRoot.clicked() }
                }

                TbButton { text: "Run";   onClicked: Evaluator.run(Editor.text) }
                TbButton { text: "Play";  onClicked: Evaluator.play(Editor.text) }
                TbButton { text: "Step";  onClicked: Evaluator.step(Editor.text) }
                TbButton { text: "Undo";  onClicked: Evaluator.undo() }
                TbButton { text: "Reset"; onClicked: { Evaluator.reset(); Editor.clearAllMarkers() } }
                Item { Layout.fillWidth: true }
                Label {
                    text: "step " + Evaluator.stepCount + (Evaluator.isRunning ? "  ●" : "")
                    color: theme.textDim
                    font.family: theme.fontFamily
                    font.pixelSize: theme.fontPx
                }
            }
        }

        // Code editor with line-count gutter (sunken bevel around it)
        Bevel {
            kind: "sunken"
            color: theme.editorBg
            Layout.fillWidth: true
            Layout.fillHeight: true
            Layout.preferredHeight: 350

            RowLayout {
                anchors.fill: parent
                spacing: 0

                // Line-count gutter + decorations
                Rectangle {
                    id: gutter
                    Layout.fillHeight: true
                    Layout.preferredWidth: 56
                    color: theme.panel

                    Flickable {
                        id: gutterFlick
                        anchors.fill: parent
                        contentY: editorFlick.contentY
                        interactive: false
                        contentHeight: gutterText.height

                        // Line numbers
                        TextEdit {
                            id: gutterText
                            width: gutter.width
                            readOnly: true
                            color: "#858585"
                            font.family: "Consolas, Menlo, monospace"
                            font.pixelSize: 14
                            horizontalAlignment: TextEdit.AlignRight
                            rightPadding: 8
                            topPadding: 8
                            text: {
                                var lines = editor.lineCount
                                var s = ""
                                for (var i = 1; i <= lines; ++i) s += i + "\n"
                                return s
                            }
                        }

                        // Click anywhere in the gutter to toggle a breakpoint
                        MouseArea {
                            anchors.fill: parent
                            onClicked: (mouse) => {
                                var lineH = gutterText.font.pixelSize + 4
                                var top = gutterText.topPadding
                                var line = Math.floor((mouse.y - top) / lineH) + 1
                                if (line >= 1 && line <= editor.lineCount) Editor.toggleBreakpoint(line)
                            }
                        }

                        // Gutter decorations (breakpoints, exec arrow, errors)
                        Repeater {
                            model: Editor.markers
                            delegate: Item {
                                width: gutter.width
                                height: gutterText.font.pixelSize + 4
                                y: gutterText.topPadding + (modelData.line - 1) * (gutterText.font.pixelSize + 4)
                                Rectangle {
                                    visible: modelData.kind === "breakpoint"
                                    width: 10; height: 10; radius: 5
                                    anchors.verticalCenter: parent.verticalCenter
                                    x: 6
                                    color: "#e51400"
                                }
                                Text {
                                    visible: modelData.kind === "executionArrow"
                                    text: "▶"
                                    color: "#ffd54a"
                                    anchors.verticalCenter: parent.verticalCenter
                                    x: 4
                                }
                                Text {
                                    visible: modelData.kind === "error"
                                    text: "✕"
                                    color: "#f48771"
                                    anchors.verticalCenter: parent.verticalCenter
                                    x: 6
                                }
                                ToolTip.visible: hover.hovered && modelData.tooltip !== ""
                                ToolTip.text: modelData.tooltip
                                HoverHandler { id: hover }
                            }
                        }
                    }
                }

                //Code editor
                Flickable {
                    id: editorFlick
                    Layout.fillWidth: true
                    Layout.fillHeight: true
                    clip: true
                    contentWidth: editor.paintedWidth
                    contentHeight: editor.paintedHeight

                    TextEdit {
                        id: editor
                        width: editorFlick.width
                        color: "#d4d4d4"
                        selectionColor: "#264f78"
                        selectedTextColor: "white"
                        font.family: "Consolas, Menlo, monospace"
                        font.pixelSize: 14
                        wrapMode: TextEdit.NoWrap
                        textFormat: TextEdit.PlainText
                        leftPadding: 8
                        topPadding: 8
                        focus: true
                        Component.onCompleted: text = Editor.text
                        onTextChanged: Editor.text = text
                        Connections {
                            target: Editor
                            function onTextChanged() {
                                if (editor.text !== Editor.text)
                                    editor.text = Editor.text
                            }
                        }
                    }
                }
            }
        }

        // Tab view (Win95-style notebook tabs on a raised strip)
        Bevel {
            kind: "flat"
            Layout.fillWidth: true
            Layout.preferredHeight: 26
            color: theme.bg

            property int currentIndex: 0
            id: tabBar

            Row {
                anchors.left: parent.left
                anchors.bottom: parent.bottom
                anchors.leftMargin: 4
                spacing: 2

                component TabBtn : Bevel {
                    id: tabRoot
                    property alias text: tlbl.text
                    property int index: 0
                    property bool active: tabBar.currentIndex === index
                    kind: "raised"
                    height: active ? 26 : 22
                    width: Math.max(70, tlbl.implicitWidth + 22)
                    y: active ? 0 : 2
                    // Active tab: paint a thin accent stripe on top...
                    Rectangle {
                        visible: tabRoot.active
                        anchors.left: parent.left; anchors.right: parent.right; anchors.top: parent.top
                        anchors.leftMargin: 2; anchors.rightMargin: 2; anchors.topMargin: 2
                        height: 2
                        color: theme.accent
                    }
                    // ...and hide the bottom bevel so the tab merges into the content below.
                    Rectangle {
                        visible: tabRoot.active
                        anchors.left: parent.left; anchors.right: parent.right; anchors.bottom: parent.bottom
                        height: 2
                        color: theme.bg
                    }
                    Label {
                        id: tlbl
                        anchors.centerIn: parent
                        anchors.verticalCenterOffset: tabRoot.active ? -1 : 0
                        color: theme.text
                        font.family: theme.fontFamily
                        font.pixelSize: theme.fontPx
                        font.bold: tabRoot.active
                    }
                    MouseArea { anchors.fill: parent; onClicked: tabBar.currentIndex = tabRoot.index }
                }

                TabBtn { text: "Console"; index: 0 }
                TabBtn { text: "AST";     index: 1 }
                TabBtn { text: "Errors";  index: 2 }
                TabBtn { text: "Help";    index: 3 }
            }
        }

        StackLayout {
            Layout.fillWidth: true
            Layout.preferredHeight: 220
            currentIndex: tabBar.currentIndex

            // Console
            Bevel {
                kind: "sunken"
                color: theme.panel
                TextArea {
                    anchors.fill: parent
                    anchors.margins: 2
                    readOnly: true
                    color: theme.text
                    font.family: "Consolas, Menlo, monospace"
                    text: Evaluator.consoleOutput.length === 0 ? "(console)" : Evaluator.consoleOutput
                    background: null
                }
            }

            // AST tree
            Bevel {
                kind: "sunken"
                color: theme.panel
                ScrollView {
                    anchors.fill: parent
                    anchors.margins: 2
                    Column {
                        width: parent.width
                        spacing: 0
                        AstNodeView { node: Evaluator.root; depth: 0 }
                    }
                }
            }

            // Errors
            Bevel {
                kind: "sunken"
                color: theme.panel
                TextArea {
                    anchors.fill: parent
                    anchors.margins: 2
                    readOnly: true
                    color: "#ff8a7a"
                    font.family: "Consolas, Menlo, monospace"
                    text: Evaluator.errorsOutput
                    background: null
                }
            }

            // Help
            Bevel {
                kind: "sunken"
                color: theme.panel
                TextArea {
                    anchors.fill: parent
                    anchors.margins: 2
                    readOnly: true
                    color: theme.text
                    font.family: theme.fontFamily
                    font.pixelSize: theme.fontPx
                    text: "Pocketknife\n\nToolbar:\n  Run   — parse, compile and evaluate the current buffer\n  Play  — finish any active stepping session\n  Step  — advance one evaluation step\n  Undo  — undo the last step's console output\n  Reset — clear stepping state and outputs\n\nClick in the gutter to toggle a breakpoint."
                    background: null
                }
            }
        }
    }
    Component {
        id: astNodeComponent
        AstNodeView {}
    }
}
