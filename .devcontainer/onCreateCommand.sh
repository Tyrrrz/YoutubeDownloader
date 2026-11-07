#!/usr/bin/env sh
set -eu

# Use XFCE4 as the default desktop environment for VNC sessions
# instead of Fluxbox which is the default for desktop-lite feature.
# XFCE4 looks a lot nicer while still keeping resource usage low.
mkdir -p "$HOME/.vnc"
cat > "$HOME/.vnc/xstartup" <<'EOF'
#!/usr/bin/env sh
unset SESSION_MANAGER
unset DBUS_SESSION_BUS_ADDRESS
exec startxfce4
EOF

chmod +x "$HOME/.vnc/xstartup"
