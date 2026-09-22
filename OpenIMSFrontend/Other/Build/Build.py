from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI



def build():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    tf.build()
    tf.add_culture_chooser(tf.get_product_name(),tf.get_available_cultures_for_angular_app())
    tf.add_maintenance_site(tf.get_product_name())


if __name__ == "__main__":
    build()
