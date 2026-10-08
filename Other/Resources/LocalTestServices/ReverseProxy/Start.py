import os
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore


def start_local_test_service():
    stript_file=__file__
    ScriptCollectionCore().start_local_test_service(stript_file)


if __name__ == "__main__":
    start_local_test_service()
