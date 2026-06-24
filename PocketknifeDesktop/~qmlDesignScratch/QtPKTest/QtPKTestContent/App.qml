import QtQuick
import QtPKTest

Window {
    width: mainScreen.width
    height: mainScreen.height

    visible: true
    title: "QtPKTest"

    Screen01 {
        id: mainScreen

        anchors.centerIn: parent
    }

}

