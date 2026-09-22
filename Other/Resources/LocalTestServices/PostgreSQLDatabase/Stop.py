import os
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore


def stop_local_test_service():
    ScriptCollectionCore().stop_local_test_service(os.path.abspath(__file__))


if __name__ == "__main__":
    stop_local_test_service()
