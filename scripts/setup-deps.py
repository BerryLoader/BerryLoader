import io
import os
from pathlib import Path
import shutil
from tempfile import TemporaryDirectory
from zipfile import ZipFile

import requests

DEPS_PATH = Path("deps")
DISCORD_GAME_SDK_URL = "https://dl-game-sdk.discordapp.net/2.5.6/discord_game_sdk.zip"


def download_discord_gamesdk():
    r = requests.get(DISCORD_GAME_SDK_URL)
    assert r.status_code == 200
    with ZipFile(io.BytesIO(r.content)) as zip_:
        with TemporaryDirectory() as tmp_:
            tmp = Path(tmp_)
            zip_.extractall(tmp)
            shutil.copytree(tmp / "csharp", DEPS_PATH, dirs_exist_ok=True)


if __name__ == "__main__":
    old_dir = os.getcwd()
    try:
        os.chdir(Path(__file__).parent.parent)
        DEPS_PATH.mkdir(exist_ok=True)
        download_discord_gamesdk()
    finally:
        os.chdir(old_dir)
