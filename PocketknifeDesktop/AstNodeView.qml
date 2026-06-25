// AstNodeView.qml
import QtQuick
import QtQuick.Controls

Column {
    property var node
    property int depth: 0
    spacing: 0

    Row {
        spacing: 4
        Item {
            width: depth * 14; height: 1
        }
        Text {
            text: (node && node.hasChildren ? (node.expanded ? "[-] " : "[+] ") : "    ") + (node ? node.label : "")
            color: "#e6e6e6"
            font.family: "MS Sans Serif, Tahoma, Microsoft Sans Serif, sans-serif"
            font.pixelSize: 12
            MouseArea {
                anchors.fill: parent
                onClicked: if (node && node.hasChildren) node.expanded = !node.expanded
            }
        }
        Text {
            visible: node && node.detail.length > 0
            text: "  ; " + (node ? node.detail.split('\n')[0] : "")
            color: "#8fbf6f"
            font.family: "MS Sans Serif, Tahoma, Microsoft Sans Serif, sans-serif"
            font.pixelSize: 12
        }
    }

    Repeater {
        model: (node && node.expanded) ? node.children : null
        delegate: Loader {
            sourceComponent: astNodeComponent
            onLoaded: {
                item.node = modelData
                item.depth = depth + 1
            }
        }
    }
}
