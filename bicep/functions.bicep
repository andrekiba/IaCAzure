@export()
func unique4() string => '${take(uniqueString(resourceGroup().id, deployment().name), 4)}'

